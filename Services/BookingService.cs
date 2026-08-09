using _Tripfinity.Interfaces;
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
    private readonly IAdminService _adminService;
    private readonly IMarshalService _marshalService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        AppDbContext context,
        IWalletService walletService,
        ITicketService ticketService,
        IAdminService adminService,
        IMarshalService marshalService,
        ILogger<BookingService> logger)
    {
        _context = context;
        _walletService = walletService;
        _ticketService = ticketService;
        _adminService = adminService;
        _marshalService = marshalService;
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

        var isMarshalCancelling = requestingUserId == null;
        var user = await _context.Users.FindAsync(booking.UserId);
        if (user == null || string.IsNullOrEmpty(user.UserWalletId))
            return Failed("Wallet not found");

        var tripTime = ResolveTripTime(booking);
        var now = DateTime.Now;

        if (tripTime > now)
        {
            if (booking.BusTrip != null) booking.BusTrip.AvailableSeats += booking.NumberOfSeats;
            else if (booking.RailwayTrip != null) booking.RailwayTrip.AvailableSeats += booking.NumberOfSeats;
            else if (booking.TaxiTrip != null) booking.TaxiTrip.AvailableSeats += booking.NumberOfSeats;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = now;
        booking.CancellationReason= reason;
        if (ticket != null) ticket.Status = TicketStatus.Cancelled;
        
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var marshalWalletId = await _marshalService.GetMarshalWalletIdAsync(booking);
            var adminWalletId = await _adminService.GetAdminWalletIdAsync();
            
            if (string.IsNullOrEmpty(marshalWalletId) || string.IsNullOrEmpty(adminWalletId))
            {
                await tx.RollbackAsync();
                return Failed("Marshal or admin wallet not found – cannot process cancellation");
            }

            var total = booking.TotalAmount;

            if (isMarshalCancelling)
            {
                var refundUser = await _walletService.CreditWalletAsync(new CreditWalletRequest
                {
                    Amount = total,
                    CustomerId = user.UserWalletId,
                    Description = $"Full refund – Marshal cancelled booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });
                
                if (refundUser.ResponseHeader.ResponseCode != "00")
                {
                    await tx.RollbackAsync();
                    return Failed("Failed to refund user");
                }
                
                var debitMarshal = await _walletService.DebitWalletAsync(new DebitWalletRequest
                {
                    Amount = total * 0.8m,
                    CustomerId = marshalWalletId,
                    Description = $"Reversal – marshal cancelled booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });

                // Take back admin's 20%
                var debitAdmin = await _walletService.DebitWalletAsync(new DebitWalletRequest
                {
                    Amount = total * 0.2m,
                    CustomerId = adminWalletId,
                    Description = $"Reversal – marshal cancelled booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });
                
                var penaltyFromMarshal = await _walletService.DebitWalletAsync(new DebitWalletRequest
                {
                    Amount = total * 0.05m,
                    CustomerId = marshalWalletId,
                    Description = $"Penalty – marshal cancelled booking ${booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });

                var penaltyToAdmin = await _walletService.CreditWalletAsync(new CreditWalletRequest
                {
                    Amount = total * 0.05m,
                    CustomerId = adminWalletId,
                    Description = $"Penalty received – marshal cancelled booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });
                
                if (debitMarshal.ResponseHeader.ResponseCode != "00" ||
                    debitAdmin.ResponseHeader.ResponseCode != "00" ||
                    penaltyFromMarshal.ResponseHeader.ResponseCode != "00" ||
                    penaltyToAdmin.ResponseHeader.ResponseCode != "00")
                {
                    await tx.RollbackAsync();
                    return Failed("One or more marshal cancellation adjustments failed");
                }
            }

            else //User cancellation
            {
                if (tripTime > now.AddHours(2))
                {
                    // >2h before departure: user gets 80% back, marshal returns 80%
                    await _walletService.CreditWalletAsync(new CreditWalletRequest
                    {
                        Amount = total * 0.8m,
                        CustomerId = user.UserWalletId,
                        Description = $"80% refund – user cancelled booking #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });

                    await _walletService.DebitWalletAsync(new DebitWalletRequest
                    {
                        Amount = total * 0.8m,
                        CustomerId = marshalWalletId,
                        Description = $"Reversal – user cancelled booking #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });
                    // admin keeps 20%
                }
                else if (now < tripTime)
                {
                    // <2h before departure: user gets 60%, marshal keeps 25% returns 55%, admin keeps 15% (returns 5%)
                    await _walletService.CreditWalletAsync(new CreditWalletRequest
                    {
                        Amount = total * 0.6m,
                        CustomerId = user.UserWalletId,
                        Description = $"60% refund – late cancellation #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });

                    await _walletService.DebitWalletAsync(new DebitWalletRequest
                    {
                        Amount = total * 0.55m,
                        CustomerId = marshalWalletId,
                        Description = $"Reversal – late cancellation #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });

                    await _walletService.DebitWalletAsync(new DebitWalletRequest
                    {
                        Amount = total * 0.05m,
                        CustomerId = adminWalletId,
                        Description = $"Reversal – late cancellation #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });
                }
                else
                {
                    // Already departed – no refund
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    return new BookingResult
                    {
                        Success = true,
                        Status = BookingStatus.Cancelled,
                        Message = "Booking cancelled; no refund for no‑show",
                        Booking = booking
                    };
                }
            }
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new BookingResult
            {
                Success = true,
                Status = BookingStatus.Cancelled,
                Message = isMarshalCancelling
                    ? "Booking cancelled by marshal; passenger fully refunded"
                    : "Booking cancelled and refund processed",
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

            if (debit.ResponseHeader.ResponseCode == "00")
            {
                booking.Status = BookingStatus.Confirmed;
                booking.PaymentTransactionId = debit.TransactionId;
                booking.PaymentTraceId = traceId;
                await _context.SaveChangesAsync();

                // payment splits
                var marshalProfit = booking.TotalAmount * 0.8m;
                var platformProfit = booking.TotalAmount * 0.2m;

                var marshaWalletId = await _marshalService.GetMarshalWalletIdAsync(booking);
                var adminWalletId = await _adminService.GetAdminWalletIdAsync();

                if (string.IsNullOrEmpty(marshaWalletId) || string.IsNullOrEmpty(adminWalletId))
                {
                    await _walletService.RefundAsync(new RefundRequest
                    {
                        CustomerId = user.UserWalletId,
                        TransactionId = debit.TransactionId,
                        Description = "Failed split - refund",
                    });

                    await transaction.RollbackAsync();
                    return Failed("Unable to process payment split - marshal or admin wallet missing");
                }
                
                var marshalCredit = await _walletService.CreditWalletAsync(new CreditWalletRequest
                {
                    Amount = marshalProfit,
                    CustomerId = marshaWalletId,
                    Description = $"Earnings from booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });

                var adminCredit = await _walletService.CreditWalletAsync(new CreditWalletRequest
                {
                    Amount = platformProfit,
                    CustomerId = adminWalletId,
                    Description = $"Commission from booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });

                if (marshalCredit.ResponseHeader.ResponseCode != "00" || adminCredit.ResponseHeader.ResponseCode != "00")
                {
                    // Reverse everything – refund user, reverse any successful credits
                    await _walletService.RefundAsync(new RefundRequest
                    {
                        CustomerId = user.UserWalletId,
                        TransactionId = debit.TransactionId,
                        Description = "Split partially failed – full refund",
                    });
                    
                }

                if (marshalCredit.ResponseHeader.ResponseCode == "00")
                { 
                    await _walletService.RefundAsync(new RefundRequest 
                    { 
                        CustomerId = user.UserWalletId, 
                        TransactionId = marshalCredit.TransactionId,
                        Description = "Reversal due to split failure",
                    });
                }

                if (adminCredit.ResponseHeader.ResponseCode == "00")
                { 
                    await _walletService.RefundAsync(new RefundRequest
                    {
                        CustomerId = user.UserWalletId,
                        TransactionId = adminCredit.TransactionId,
                        Description = "Reversal due to split failure",
                    });
                    await transaction.RollbackAsync();
                    return Failed("Payment split failed; booking cancelled, wallet refunded");
                }
                
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

            if (debit.ResponseHeader.ResponseCode == "01")
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
                Message = debit.ResponseHeader.ResponseMessage
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Booking failed for user {UserId}", user.Id);
            throw;
        }
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