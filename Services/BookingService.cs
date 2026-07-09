using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;
public enum TransportType
{
    Railway,
    Taxi,
    Bus
}
    
public enum TicketStatus
{
    Issued,
    Validated,
    Expired,
    Cancelled,
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Failed
}

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

    public async Task<Booking?> GetBookingAsync(int id, TransportType transportType)
    {
        return await _context.Bookings
            .Include(booking => booking.User)
            .FirstOrDefaultAsync(booking => booking.Id == id && booking.TransportType == transportType);
    }

    public Task<List<Booking>> GetRecentBookings(int userId, string transportType)
    {
        throw new NotImplementedException();
    }

    public async Task<BookingResult> BookBusAsync(int tripId, int seats, int? userId)
    {
        var user = await _context.Users.FindAsync(userId);
        var trip = await _context.BusTrips.FindAsync(tripId);
        if (user == null || trip == null) 
            return Failed("Trip or user not found");
        if (seats < 1) 
            return Failed("Invalid number of seats");
        if (seats > trip.AvailableSeats) 
            return Failed("Not enough available seats");

        var booking = new Booking
        {
            UserId = user.Id,
            BusTripId = tripId,
            BusTrip = trip,
            TransportType = TransportType.Bus,
            NumberOfSeats = seats,
            TotalAmount = trip.Price * seats,
            Status = BookingStatus.Pending,
            BookingDate = DateTime.Now
        };

        return await ProcessBookingAsync(user, booking, () => trip.AvailableSeats -= seats);
    }

    public async Task<BookingResult> BookRailwayAsync(int tripId, int seats, int? userId)
    {
        var user = await _context.Users.FindAsync(userId);
        var trip = await _context.RailwayTrips.FindAsync(tripId);
        if (user == null || trip == null) 
            return Failed("Trip or user not found");
        if (seats < 1) 
            return Failed("Invalid number of seats");
        if (seats > trip.AvailableSeats) 
            return Failed("Not enough available seats");

        var booking = new Booking
        {
            UserId = user.Id,
            RailwayTripId = tripId,
            RailwayTrip = trip,
            TransportType = TransportType.Railway,
            NumberOfSeats = seats,
            TotalAmount = trip.Price * seats,
            Status = BookingStatus.Pending,
            BookingDate = DateTime.Now
        };

        return await ProcessBookingAsync(user, booking, () => trip.AvailableSeats -= seats);
    }

    public async Task<BookingResult> BookTaxiAsync(int tripId, int seats, int? userId)
    {
        var user = await _context.Users.FindAsync(userId);
        var trip = await _context.TaxiTrips.FindAsync(tripId);
        if (user == null || trip == null) 
            return Failed("Trip or user not found");
        if (seats < 1) 
            return Failed("Invalid number of seats");
        if (seats > trip.MaxPassengers) 
            return Failed("Seats requested exceed taxi capacity");

        var booking = new Booking
        {
            UserId = user.Id,
            TaxiTripId = tripId,
            TaxiTrip = trip,
            TransportType = TransportType.Taxi,
            NumberOfSeats = seats,
            TotalAmount = trip.Price, // taxi is flat-rate per ride
            Status = BookingStatus.Pending,
            BookingDate = DateTime.Now
        };

        // taxis have no shared-seat inventory to decrement
        return await ProcessBookingAsync(user, booking, () => { });
    }

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
            return Failed("You are not authorized to cancel this booking");
        if (booking.Status == BookingStatus.Cancelled) 
            return Failed("Booking is already cancelled");

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.BookingId == bookingId);
        
        // A validated ticket means the trip was already taken — not cancellable.
        if (ticket is { Status: TicketStatus.Validated })
            return Failed("Ticket has already been used and cannot be cancelled");

        var tripTime = ResolveTripTime(booking);
        var eligibleForRefund = booking.Status == BookingStatus.Confirmed && tripTime > DateTime.Now.AddHours(2);

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // Restore inventory for future trips (bus/railway only).
            if (tripTime > DateTime.Now)
            {
                if (booking.BusTrip != null) booking.BusTrip.AvailableSeats += booking.NumberOfSeats;
                else if (booking.RailwayTrip != null) booking.RailwayTrip.AvailableSeats += booking.NumberOfSeats;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.Now;
            booking.CancellationReason = reason;

            if (eligibleForRefund)
            {
                if (ticket != null) 
                    ticket.Status = TicketStatus.Cancelled;
                await _context.SaveChangesAsync();

                var refunded = await RefundAsync(booking);
                await tx.CommitAsync();

                return new BookingResult
                {
                    Success = true,
                    Status = BookingStatus.Cancelled,
                    Message = refunded
                        ? "Booking cancelled and wallet refunded"
                        : "Booking cancelled. Refund could not be processed",
                    Booking = booking
                };
            }

            // Not eligible for a refund: delete the issued ticket outright.
            // if (ticket != null) _context.Tickets.Remove(ticket);
            // await _context.SaveChangesAsync();
            // await tx.CommitAsync();

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

    public async Task<List<Booking>> GetRecentBookings(int userId, TransportType transportType)
    {
        return await _context.Bookings
            .Where(b => b.UserId == userId)
            .Where(b => b.TransportType == transportType)
            .OrderByDescending(b => b.BookingDate)
            .Take(5)
            .ToListAsync();
    }
    
    private async Task<BookingResult> ProcessBookingAsync(User user, Booking booking, Action applySeatChange)
    {
        if (string.IsNullOrEmpty(user.UserWalletId))
            return Failed("No wallet is linked to this account. Confirm your email to create a wallet.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            applySeatChange();
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(); // assigns booking.Id

            var traceId = Guid.NewGuid().ToString("N");
            var debit = await _walletService.DebitWalletAsync(new DebitWalletRequest
            {
                Amount = booking.TotalAmount,
                CustomerId = user.UserWalletId,
                Description = $"Tripfinity {booking.TransportType} booking #{booking.Id}",
                TraceId = traceId
            });

            var code = debit?.ResponseHeader?.ResponseCode;

            if (code == "00")
            {
                booking.Status = BookingStatus.Confirmed;
                booking.PaymentTransactionId = debit!.TransactionId;
                booking.PaymentTraceId = traceId;
                await _context.SaveChangesAsync();

                var ticket = await _ticketService.IssueTicketAsync(booking);
                await tx.CommitAsync();

                return new BookingResult
                {
                    Success = true,
                    Status = BookingStatus.Confirmed,
                    Message = "Booking confirmed and ticket issued",
                    Booking = booking,
                    Ticket = ticket
                };
            }

            // Payment did not succeed — undo the booking and seat reservation.
            await tx.RollbackAsync();

            if (code == "01")
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
            await tx.RollbackAsync();
            _logger.LogError(ex, "Booking failed for user {UserId}", user.Id);
            throw;
        }
    }

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

            return credit?.ResponseHeader?.ResponseCode == "00";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refund failed for booking {BookingId}", booking.Id);
            return false;
        }
    }

    private static DateTime ResolveTripTime(Booking booking)
    {
        if (booking.BusTrip != null) return booking.BusTrip.DepartureTime;
        if (booking.RailwayTrip != null) return booking.RailwayTrip.DepartureTime;
        if (booking.TaxiTrip != null) return booking.TaxiTrip.PickupTime;
        return DateTime.Now;
    }

    private static BookingResult Failed(string message) =>
        new ()
        {
            Success = false, 
            Status = BookingStatus.Failed, 
            Message = message,
        };
}
