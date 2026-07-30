using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class PassengerService : IPassengerService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _wallet;
    private readonly ILogger<PassengerService> _logger;
    private readonly ITicketService _ticketService;

    public PassengerService(AppDbContext context, IWalletService wallet, ILogger<PassengerService> logger, ITicketService ticketService)
    {
        _context = context;
        _wallet = wallet;
        _logger = logger;
        _ticketService = ticketService;
    }

    public async Task<User?> GetPassengerAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user is { Role: "Passenger" } ? user : null;
    }

    public async Task<decimal> GetWalletBalanceAsync(string? walletId)
    {
        if (string.IsNullOrWhiteSpace(walletId)) return 0;
        try
        {
            var resp = await _wallet.GetBalanceAsync(new GetBalanceRequest { CustomerId = walletId });
            return resp?.Balance ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch wallet balance for {WalletId}", walletId);
            return 0;
        }
    }

    public async Task<List<Booking>> GetUpcomingBookingsAsync(int userId) =>
        await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Where(b => b.UserId == userId && b.BookingDate > DateTime.Now)
            .OrderByDescending(b => b.BookingDate)
            .Take(10)
            .ToListAsync();
    
    // New methods to add to the existing PassengerService class

    public async Task<TicketIndexViewModel> GetTicketIndexAsync(int userId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        var tickets = await _context.Tickets
            .Where(t => t.PassengerId == userId)
            .ToListAsync();

        return new TicketIndexViewModel { Bookings = bookings, Tickets = tickets };
    }

    public async Task<BookingDetailViewModel?> GetBookingDetailAsync(int bookingId, int userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

        if (booking is null) return null;

        var ticket = await _ticketService.GetTicketByBookingAsync(booking.Id);
        return new BookingDetailViewModel { Booking = booking, Ticket = ticket };
    }
}

