using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class MarshalService : IMarshalService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _wallet;

    public MarshalService(AppDbContext context, IWalletService wallet)
    {
     _context = context;
     _wallet = wallet;

    }
        

    public async Task<User?> GetMarshalAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user is { Role: "Marshal" } ? user : null;
    }

    public async Task<object?> GetMarshalTripsAsync(int marshalId, string vehicleType)
    {
        return vehicleType switch
        {
            "Bus" => await _context.BusTrips
                .Where(t => t.MarshalId == marshalId)
                .OrderByDescending(t => t.DepartureTime)
                .ToListAsync(),
            "Railway" => await _context.RailwayTrips
                .Where(t => t.MarshalId == marshalId)
                .OrderByDescending(t => t.DepartureTime)
                .ToListAsync(),
            "Taxi" => await _context.TaxiTrips
                .Where(t => t.MarshalId == marshalId)
                .OrderByDescending(t => t.PickupTime)
                .ToListAsync(),
            _ => null
        };
    }
    
    public async Task<string?> GetMarshalWalletIdAsync(Booking booking)
    {
        var marshalId = booking.BusTrip?.MarshalId
                        ?? booking.RailwayTrip?.MarshalId
                        ?? booking.TaxiTrip?.MarshalId;
        if (marshalId == null) return null;
        var marshal = await _context.Users.FindAsync(marshalId.Value);
        return marshal?.UserWalletId;
    }
    
     public async Task<MarshalWalletViewModel> GetWalletInfoAsync(int userId, int page = 1)
        {
            var marshal = await _context.Users.FindAsync(userId);
            if (marshal is not { Role: "Marshal" } || string.IsNullOrWhiteSpace(marshal.UserWalletId))
                return null;

            var balanceResp = await _wallet.GetBalanceAsync(new GetBalanceRequest { CustomerId = marshal.UserWalletId });
            var balance = balanceResp?.Balance ?? 0;

            var transactions = new List<TransactionDetailsList>();
            var totalPages = 1;
            bool hasNext = false, hasPrev = false;

            var listResp = await  _wallet.GetTransactionList(new GetTransactionListRequest
            {
                CustomerId = marshal.UserWalletId,
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
                WalletId = marshal.UserWalletId,
                Balance = balance,
                Transactions = transactions,
                CurrentPage = page,
                TotalPages = totalPages,
                HasNext = hasNext,
                HasPrevious = hasPrev
            };
        }
}