using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AdminService : IAdminService
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

    public Task<MarshalWalletViewModel> GetAdminWalletInfoAsync(string walletId, int page) =>
        _walletService.BuildWalletInfoAsync(walletId, page);

    public async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.Role == "Admin";
    }

    public async Task<List<T>> GetAllTripsAsync<T>() where T : class, ITrip
    {
        var trips = await _context.Set<T>().ToListAsync();
        return trips.OrderByDescending(trip=> trip.CreatedAt).ToList();
    }
    
    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.BusTrip)
            .Include(b => b.RailwayTrip)
            .Include(b => b.TaxiTrip)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }
}