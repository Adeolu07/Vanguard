using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AdminService: IAdminService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;

    public AdminService(AppDbContext context, IWalletService walletService)
    {
        _context = context;
        _walletService = walletService;
    }
    public async Task<string?> GetAdminWalletIdAsync()
    {
        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Role == "Admin");
        return admin?.UserWalletId;
    }

    public async Task<MarshalWalletViewModel> GetAdminWalletInfoAsync(string walletId, int page)
    {
        // Reuse existing wallet service calls similar to MarshalService.GetWalletInfoAsync
        var balanceResp = await _walletService.GetBalanceAsync(new GetBalanceRequest { CustomerId = walletId });
        var balance = balanceResp?.Balance ?? 0;

        var transactions = new List<TransactionDetailsList>();
        var totalPages = 1;
        bool hasNext = false, hasPrev = false;

        var listResp = await _walletService.GetTransactionList(new GetTransactionListRequest
        {
            CustomerId = walletId,
            SearchDetails = new SearchDetails
            {
                Page = page,
                ItemsPerPage = 10,
                DateRange = new DateRange { Start = DateTime.Now.AddMonths(-3), End = DateTime.Now }
            }
        });

        if (listResp?.TransactionDetailsList != null)
        {
            transactions = listResp.TransactionDetailsList
                .Select(d => new TransactionDetailsList
                {
                    TranType = d.TranType,
                    Amount = d.Amount,
                    Description = d.Description,
                    TransactionId = d.TransactionId,
                    SessionId = d.SessionId
                }).ToList();
            totalPages = listResp.Pagination?.TotalPages ?? 1;
            hasNext = listResp.Pagination?.HasNext ?? false;
            hasPrev = listResp.Pagination?.HasPrevious ?? false;
        }

        return new MarshalWalletViewModel
        {
            WalletId = walletId,
            Balance = balance,
            Transactions = transactions,
            CurrentPage = page,
            TotalPages = totalPages,
            HasNext = hasNext,
            HasPrevious = hasPrev
        };
    }
    
    public async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.Role == "Admin";
    }

    public Task<List<BusTrip>> GetAllBusTripsAsync() =>
        _context.BusTrips.OrderByDescending(t => t.CreatedAt).ToListAsync();

    public Task<List<RailwayTrip>> GetAllRailwayTripsAsync() =>
        _context.RailwayTrips.OrderByDescending(t => t.CreatedAt).ToListAsync();

    public Task<List<TaxiTrip>> GetAllTaxiTripsAsync() =>
        _context.TaxiTrips.OrderByDescending(t => t.CreatedAt).ToListAsync();

    public Task<List<Booking>> GetAllBookingsAsync() =>
        _context.Bookings
            .Include(b => b.User)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    
    
}