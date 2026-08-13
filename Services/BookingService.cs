using _Tripfinity.Interfaces;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;
public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly ITicketService _ticketService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        AppDbContext context,
        ITicketService ticketService,
        IPaymentService paymentService,
        ILogger<BookingService> logger)
    {
        _context = context;
        _ticketService = ticketService;
        _paymentService = paymentService;
        _logger = logger;
    }
    
    public async Task<Booking?> GetBookingAsync(int id, TransportType transportType) =>
        await _context.Bookings
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id && b.TransportType == transportType);

    public async Task<List<Booking>> GetFiveRecentBookings(int userId, TransportType transportType) =>
        await _context.Bookings
            .Where(b => b.UserId == userId && b.TransportType == transportType)
            .OrderByDescending(b => b.BookingDate)
            .Take(5)
            .ToListAsync();

    public async Task<object?> GetTripAsync(string type, int tripId) =>
        type.ToLower() switch
        {
            "bus"     => await _context.BusTrips.FindAsync(tripId),
            "railway" => await _context.RailwayTrips.FindAsync(tripId),
            "taxi"    => await _context.TaxiTrips.FindAsync(tripId),
            _         => null
        };

    public async Task<BookingResult> BookAsync(string type, int tripId, int seats, int userId) =>
        type.ToLower() switch
        {
            "bus"     => await BookTripAsync<BusTrip>(tripId, seats, userId, TransportType.Bus),
            "railway" => await BookTripAsync<RailwayTrip>(tripId, seats, userId, TransportType.Railway),
            "taxi"    => await BookTaxiAsync(tripId, seats, userId), // special flat‑rate logic
            _         => new BookingResult { Success = false, Message = "Invalid transport type" }
        };

    // ─── Unified bus/railway booking ─────────────────────────────────────

    private async Task<BookingResult> BookTripAsync<T>(int tripId, int seats, int userId,
        TransportType transportType) where T : class
    {
        var user = await _context.Users.FindAsync(userId);
        var trip = await _context.FindAsync<T>(tripId);
        if (user == null || trip == null)
            return Failed("Trip or user not found");
        if (seats < 1)
            return Failed("Invalid number of seats");

        // Reflection‑free seat access
        var available = GetAvailableSeats(trip);
        if (seats > available)
            return Failed("Not enough available seats");

        var booking = new Booking
        {
            UserId = user.Id,
            TransportType = transportType,
            NumberOfSeats = seats,
            TotalAmount = GetPrice(trip) * seats,
            Status = BookingStatus.Pending,
            BookingDate = DateTime.Now
        };

        // Set FK
        switch (transportType)
        {
            case TransportType.Bus:
                booking.BusTripId = tripId;
                booking.BusTrip = trip as BusTrip;
                break;
            case TransportType.Railway: 
                booking.RailwayTripId = tripId;  
                booking.RailwayTrip = trip as RailwayTrip;
                break;
        }

        return await ProcessBookingAsync(user, booking, () => SetAvailableSeats(trip, available - seats));
    }

    // Taxi remains special because of flat‑rate pricing and no seat inventory
    private async Task<BookingResult> BookTaxiAsync(int tripId, int seats, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        var trip = await _context.TaxiTrips.FindAsync(tripId);
        var availableSeats = trip!.AvailableSeats;
        
        if (user == null)
            return Failed("User not found");
        if (seats < 1)
            return Failed("Invalid number of seats");
        if (seats > availableSeats)
            return Failed("Not enough seats");

        var booking = new Booking
        {
            UserId = user.Id,
            TaxiTripId = tripId,
            TaxiTrip = trip,
            TransportType = TransportType.Taxi,
            NumberOfSeats = seats,
            TotalAmount = trip.Price, // flat‑rate
            Status = BookingStatus.Pending,
            BookingDate = DateTime.Now
        };

        return await ProcessBookingAsync(user, booking, () =>
        {
            trip.AvailableSeats -= seats;
        });
    }

    // ─── Cancellation ───────────────────────────────────────────────────

    public async Task<BookingResult> CancelBookingAsync(int bookingId, int? requestingUserId, string reason)
    {
        var booking = await _context.Bookings
        .Include(b => b.BusTrip)
        .Include(b => b.RailwayTrip)
        .Include(b => b.TaxiTrip)
        .FirstOrDefaultAsync(b => b.Id == bookingId);

    if (booking == null) 
        return Failed("Booking not found");
    if (requestingUserId != null && booking.UserId != requestingUserId)
        return Failed("Not authorized");
    if (booking.Status == BookingStatus.Cancelled)
        return Failed("Already cancelled");

    var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.BookingId == bookingId);
    if (ticket is { Status: TicketStatus.Validated })
        return Failed("Ticket already used and cannot be cancelled");

    var user = await _context.Users.FindAsync(booking.UserId);
    if (user == null || string.IsNullOrEmpty(user.UserWalletId))
        return Failed("Wallet not found");

    var tripTime = ResolveTripTime(booking);
    var now = DateTime.Now;
    
    // Process payment cancellation
    var isMarshalCancelling = requestingUserId == null;
    var paymentResult = await _paymentService.ProcessCancellationAsync(booking, user, isMarshalCancelling, tripTime);

    string message;

    if (!paymentResult.Refunded)
    {
        message = "Booking cancelled, no refund for no-show";
    }
    else if (isMarshalCancelling)
    {
        message = "Booking cancelled by marshal, passenger fully refunded";
    }
    else
    {
        message = "Booking cancelled and refund processed";
    }
    
    if (!paymentResult.Success)
        return Failed(paymentResult.ErrorMessage ?? "Cancellation failed");

    // if trip has not started, open a seat back up
    if (tripTime > now)
        UndoSeatChange(booking);
    
    
    // Finalize booking cancellation
    booking.CancelledAt = now;
    booking.CancellationReason = reason;
    booking.Status = BookingStatus.Cancelled;
    ticket?.Status = TicketStatus.Cancelled;

    await _context.SaveChangesAsync();
    return new BookingResult
    {
        Success = true,
        Status = BookingStatus.Cancelled,
        Message = message,
        Booking = booking
    }; 
    }
    

    // Core booking
    private async Task<BookingResult> ProcessBookingAsync(User user, Booking booking, Action applySeatChange)
    {
        if (string.IsNullOrEmpty(user.UserWalletId))
            return Failed("No wallet is linked to this account.");

        applySeatChange();
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var payResult = await _paymentService.ProcessPaymentAsync(user, booking);

        if (!payResult.Success)
        {
            UndoSeatChange(booking);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return Failed(payResult.ErrorMessage ?? "Payment failed");
        }

        // Payment succeeded
        booking.Status = BookingStatus.Confirmed;
        await _context.SaveChangesAsync();

        var vehicleId = ResolveVehicleId(booking);
        var ticket = await _ticketService.IssueTicketAsync(booking, vehicleId);

        return new BookingResult
        {
            Success = true,
            Status = BookingStatus.Confirmed,
            Message = "Booking confirmed and ticket issued",
            Booking = booking,
            Ticket = ticket
        };
        
    }
    
    // ─── Helpers ────────────────────────────────────────────────────────
    
    private void UndoSeatChange(Booking booking)
    {
        if (booking.BusTrip != null)
            booking.BusTrip.AvailableSeats += booking.NumberOfSeats;
        else if (booking.RailwayTrip != null)
            booking.RailwayTrip.AvailableSeats += booking.NumberOfSeats;
        else if (booking.TaxiTrip != null)
            booking.TaxiTrip.AvailableSeats += booking.NumberOfSeats;
    }
    private static BookingResult Failed(string message) =>
        new() { Success = false, Status = BookingStatus.Failed, Message = message };

    private static DateTime ResolveTripTime(Booking booking)
    {
        if (booking.BusTrip != null) 
            return booking.BusTrip.DepartureTime;
        if (booking.RailwayTrip != null) 
            return booking.RailwayTrip.DepartureTime;
        if (booking.TaxiTrip != null) 
            return booking.TaxiTrip.PickupTime;
        return DateTime.Now;
    }

    // Tiny reflection‑free accessors for the unified booker
    private static int GetAvailableSeats<T>(T trip) =>
        trip switch
        {
            BusTrip b => b.AvailableSeats,
            RailwayTrip r => r.AvailableSeats,
            TaxiTrip t => t.AvailableSeats,
            _ => throw new InvalidOperationException("Unsupported trip type")
        };

    private static decimal GetPrice<T>(T trip) =>
        trip switch
        {
            BusTrip b => b.Price,
            RailwayTrip r => r.Price,
            _ => throw new InvalidOperationException("Unsupported trip type")
        };

    private static void SetAvailableSeats<T>(T trip, int value)
    {
        switch (trip)
        {
            case BusTrip b: 
                b.AvailableSeats = value;
                break;
            case RailwayTrip r:
                r.AvailableSeats = value;
                break;
            case TaxiTrip t:
                t.AvailableSeats = value;
                break;
        }
    }
    
    private static string? ResolveVehicleId(Booking booking)
    {
        if (booking.BusTrip != null) return booking.BusTrip.VehicleId;
        if (booking.RailwayTrip != null) return booking.RailwayTrip.VehicleId;
        if (booking.TaxiTrip != null) return booking.TaxiTrip.VehicleId;
        return null;
    }
    
}