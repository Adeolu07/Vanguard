using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public enum TransportType { Railway, Taxi, Bus }
public enum TicketStatus { Issued, Validated, Expired, Cancelled }
public enum BookingStatus { Pending, Confirmed, Cancelled, Failed }

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly ITicketService _ticketService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        AppDbContext context,
        IWalletService walletService,
        ITicketService ticketService,
        ILogger<BookingService> logger)
    {
        _context = context;
        _walletService = walletService;
        _ticketService = ticketService;
        _logger = logger;
    }
    
    public async Task<Booking?> GetBookingAsync(int id, TransportType transportType) =>
        await _context.Bookings
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id && b.TransportType == transportType);

    public async Task<List<Booking>> GetRecentBookings(int userId, TransportType transportType) =>
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

        if (booking == null) return Failed("Booking not found");
        if (requestingUserId != null && booking.UserId != requestingUserId)
            return Failed("You are not authorized to cancel this booking");
        if (booking.Status == BookingStatus.Cancelled)
            return Failed("Booking is already cancelled");

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.BookingId == bookingId);
        if (ticket is { Status: TicketStatus.Validated })
            return Failed("Ticket has already been used and cannot be cancelled");

        var tripTime = ResolveTripTime(booking);
        var eligibleForRefund = booking.Status == BookingStatus.Confirmed &&
                                tripTime > DateTime.Now.AddHours(2);

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // Restore inventory if trip hasn't yet left
            if (tripTime > DateTime.Now)
            {
                if (booking.BusTrip != null)
                {
                    booking.BusTrip.AvailableSeats += booking.NumberOfSeats;
                }
                else if (booking.RailwayTrip != null)
                {
                    booking.RailwayTrip.AvailableSeats += booking.NumberOfSeats;
                }
                else if(booking.TaxiTrip != null)
                {
                    booking.TaxiTrip?.AvailableSeats += booking.NumberOfSeats;
                }
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.Now;
            booking.CancellationReason = reason;

            if (eligibleForRefund)
            {
                if (ticket != null) ticket.Status = TicketStatus.Cancelled;
                await _context.SaveChangesAsync();

                var refundOk = await RefundAsync(booking);
                await tx.CommitAsync();

                return new BookingResult
                {
                    Success = true,
                    Status = BookingStatus.Cancelled,
                    Message = refundOk
                        ? "Booking cancelled and wallet refunded"
                        : "Booking cancelled. Refund could not be processed",
                    Booking = booking
                };
            }

            // No refund – just cancel
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new BookingResult
            {
                Success = true,
                Status = BookingStatus.Cancelled,
                Message = "Booking cancelled. Not eligible for a refund.",
                Booking = booking
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Cancellation failed for booking {BookingId}", bookingId);
            throw;
        }
    }

    // ─── Core booking engine ────────────────────────────────────────────

    private async Task<BookingResult> ProcessBookingAsync(User user, Booking booking, Action applySeatChange)
    {
        if (string.IsNullOrEmpty(user.UserWalletId))
            return Failed("No wallet is linked to this account. Confirm your email to create a wallet.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            applySeatChange();
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var traceId = Guid.NewGuid().ToString("N");
            var debit = await _walletService.DebitWalletAsync(new DebitWalletRequest
            {
                Amount = booking.TotalAmount,
                CustomerId = user.UserWalletId,
                Description = $"Tripfinity {booking.TransportType} booking #{booking.Id}",
                TraceId = traceId
            });

            if (debit?.ResponseHeader?.ResponseCode == "00")
            {
                booking.Status = BookingStatus.Confirmed;
                booking.PaymentTransactionId = debit.TransactionId;
                booking.PaymentTraceId = traceId;
                await _context.SaveChangesAsync();
                var vehicleId = ResolveVehicleId(booking);
                var ticket = await _ticketService.IssueTicketAsync(booking, vehicleId);
                await transaction.CommitAsync();

                return new BookingResult
                {
                    Success = true,
                    Status = BookingStatus.Confirmed,
                    Message = "Booking confirmed and ticket issued",
                    Booking = booking,
                    Ticket = ticket
                };
            }

            await transaction.RollbackAsync();

            if (debit?.ResponseHeader?.ResponseCode == "01")
                return new BookingResult
                {
                    Success = false,
                    Status = BookingStatus.Cancelled,
                    Message = "Insufficient wallet balance. Please fund your wallet and try again."
                };

            return new BookingResult
            {
                Success = false,
                Status = BookingStatus.Failed,
                Message = debit?.ResponseHeader?.ResponseMessage ?? "Payment could not be processed"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Booking failed for user {UserId}", user.Id);
            throw;
        }
    }

    // ─── Refund ─────────────────────────────────────────────────────────

    private async Task<bool> RefundAsync(Booking booking)
    {
        var user = await _context.Users.FindAsync(booking.UserId);
        if (user == null || string.IsNullOrEmpty(user.UserWalletId))
            return false;

        try
        {
            var credit = await _walletService.CreditWalletAsync(new CreditWalletRequest
            {
                Amount = booking.TotalAmount,
                CustomerId = user.UserWalletId,
                Description = $"Refund for cancelled booking #{booking.Id}",
                TraceId = Guid.NewGuid().ToString("N")
            });

            if (credit?.ResponseHeader?.ResponseCode != "00") 
                return false;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refund failed for booking {BookingId}", booking.Id);
            return false;
        }
    }

    // ─── Transaction audit ──────────────────────────────────────────────
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