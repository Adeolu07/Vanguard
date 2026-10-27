using Tripfinity.Interfaces;
using Tripfinity.Models.Tables;
using Tripfinity.Models.Data;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Tripfinity.Services;
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
    
    public async Task<Booking?> GetBookingAsync(int id, TransportType transportType, int? ownerUserId = null) =>
        await _context.Bookings
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id
                                      && b.TransportType == transportType
                                      && (ownerUserId == null || b.UserId == ownerUserId));

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
        if (seats < 1)
            return Failed("Invalid number of seats");

        var user = await _context.Users.FindAsync(userId);
        var trip = await _context.FindAsync<T>(tripId);
        if (user == null || trip == null)
            return Failed("Trip or user not found");
        if (string.IsNullOrEmpty(user.UserWalletId))
            return Failed("No wallet is linked to this account.");

        // Seat inventory is claimed with a single conditional UPDATE, so two
        // concurrent bookings can never both take the last seat.
        if (!await TryReserveSeatsAsync(transportType, tripId, seats))
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

        return await ProcessBookingAsync(user, booking, transportType, tripId, seats);
    }

    // Taxi remains special because of flat‑rate pricing and no seat inventory
    private async Task<BookingResult> BookTaxiAsync(int tripId, int seats, int userId)
    {
        if (seats < 1)
            return Failed("Invalid number of seats");

        var user = await _context.Users.FindAsync(userId);
        var trip = await _context.TaxiTrips.FindAsync(tripId);

        if (user == null || trip == null)
            return Failed("Trip or user not found");
        if (string.IsNullOrEmpty(user.UserWalletId))
            return Failed("No wallet is linked to this account.");

        if (!await TryReserveSeatsAsync(TransportType.Taxi, tripId, seats))
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

        return await ProcessBookingAsync(user, booking, TransportType.Taxi, tripId, seats);
    }

    /// <summary>
    /// Atomically claims seats. Returns false when the trip disappeared or no
    /// longer has enough inventory, which is what protects against overselling.
    /// </summary>
    private async Task<bool> TryReserveSeatsAsync(TransportType transportType, int tripId, int seats)
    {
        var rows = transportType switch
        {
            TransportType.Bus => await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE [BusTrips] SET [AvailableSeats] = [AvailableSeats] - {seats} WHERE [Id] = {tripId} AND [AvailableSeats] >= {seats}"),
            TransportType.Railway => await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE [RailwayTrips] SET [AvailableSeats] = [AvailableSeats] - {seats} WHERE [Id] = {tripId} AND [AvailableSeats] >= {seats}"),
            TransportType.Taxi => await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE [TaxiTrips] SET [AvailableSeats] = [AvailableSeats] - {seats} WHERE [Id] = {tripId} AND [AvailableSeats] >= {seats}"),
            _ => 0
        };

        return rows == 1;
    }

    private async Task ReleaseSeatsAsync(TransportType transportType, int tripId, int seats)
    {
        switch (transportType)
        {
            case TransportType.Bus:
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE [BusTrips] SET [AvailableSeats] = [AvailableSeats] + {seats} WHERE [Id] = {tripId}");
                break;
            case TransportType.Railway:
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE [RailwayTrips] SET [AvailableSeats] = [AvailableSeats] + {seats} WHERE [Id] = {tripId}");
                break;
            case TransportType.Taxi:
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE [TaxiTrips] SET [AvailableSeats] = [AvailableSeats] + {seats} WHERE [Id] = {tripId}");
                break;
        }
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
    var tripId = booking.BusTripId ?? booking.RailwayTripId ?? booking.TaxiTripId;
    if (tripTime > now && tripId is not null)
        await ReleaseSeatsAsync(booking.TransportType, tripId.Value, booking.NumberOfSeats);
    
    
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
    private async Task<BookingResult> ProcessBookingAsync(User user, Booking booking,
        TransportType transportType, int tripId, int seats)
    {
        if (string.IsNullOrEmpty(user.UserWalletId))
        {
            await ReleaseSeatsAsync(transportType, tripId, seats);
            return Failed("No wallet is linked to this account.");
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var payResult = await _paymentService.ProcessPaymentAsync(user, booking);

        if (!payResult.Success)
        {
            await ReleaseSeatsAsync(transportType, tripId, seats);
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

    // Tiny reflection‑free accessor for the unified booker
    private static decimal GetPrice<T>(T trip) =>
        trip switch
        {
            BusTrip b => b.Price,
            RailwayTrip r => r.Price,
            _ => throw new InvalidOperationException("Unsupported trip type")
        };

    private static string? ResolveVehicleId(Booking booking)
    {
        if (booking.BusTrip != null) return booking.BusTrip.VehicleId;
        if (booking.RailwayTrip != null) return booking.RailwayTrip.VehicleId;
        if (booking.TaxiTrip != null) return booking.TaxiTrip.VehicleId;
        return null;
    }
    
}