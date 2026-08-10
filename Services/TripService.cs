using System.Transactions;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Enums;
using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class TripService : ITripService
{
    private readonly AppDbContext _context;
    private readonly IBookingService _bookingService;
    private readonly ILogger<TripService> _logger;

    public TripService(AppDbContext context, IBookingService bookingService, ILogger<TripService> logger)
    {
        _context = context;
        _bookingService = bookingService;
        _logger = logger;
    }

    // ... existing code (all previous methods stay unchanged) ...

    // ── High‑level creators with DTO mapping ──
    public async Task<BusTrip> CreateBusTripAsync(CreateBusTripRequest request, int marshalId, string vehicleId)
    {
        if (request.AvailableSeats > request.TotalSeats)
            throw new ArgumentException("Available seats cannot exceed total seats");
        var trip = new BusTrip
        {
            From = request.From,
            Destination = request.Destination,
            Price = request.Price,
            TotalSeats = request.TotalSeats,
            AvailableSeats = request.AvailableSeats,
            DepartureTime = request.DepartureTime,
            MarshalId = marshalId,
            VehicleId = vehicleId
        };
        
        _context.BusTrips.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task<RailwayTrip> CreateRailwayTripAsync(CreateRailwayTripRequest request, int marshalId, string vehicleId)
    {
        if (request.AvailableSeats > request.TotalSeats)
            throw new ArgumentException("Available seats cannot exceed total seats");
        var trip = new RailwayTrip
        {
            From = request.From,
            Destination = request.Destination,
            Price = request.Price,
            TotalSeats = request.TotalSeats,
            AvailableSeats = request.AvailableSeats,
            DepartureTime = request.DepartureTime,
            MarshalId = marshalId,
            VehicleId = vehicleId
        };

        _context.RailwayTrips.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task<TaxiTrip> CreateTaxiTripAsync(CreateTaxiTripRequest request, int marshalId, string vehicleId)
    {
        if (request.NumberOfPassengers > 4)
            throw new ArgumentException("Passengers cannot exceed 4.");
        var trip = new TaxiTrip
        {
            PickupLocation = request.PickupLocation,
            DropoffLocation = request.DropoffLocation,
            Price = request.Price,
            MaxPassengers = request.NumberOfPassengers,
            AvailableSeats = request.NumberOfPassengers,
            PickupTime = request.PickupTime,
            MarshalId = marshalId,
            VehicleId = vehicleId
        };

        _context.TaxiTrips.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
        
    }
// ... rest of existing code ...

    public async Task<bool> CancelTripAsync(TransportType transportType, int tripId, int marshalId, string reason)
    {
        var found = await DeactivateTripAsync(transportType, tripId, marshalId);
        if (!found) 
            return false;

        var activeBookings = await GetActiveBookingsAsync(transportType, tripId);
        foreach (var booking in activeBookings)
        {
            // System-initiated cancel (requestingUserId = null) refunds the passenger when eligible.
            await _bookingService.CancelBookingAsync(booking.Id, null, reason);
        }

        _logger.LogInformation(
            "Marshal {MarshalId} cancelled {Type} trip {TripId}; {Count} booking(s) processed",
            marshalId, transportType, tripId, activeBookings.Count);
        return true;
    }
    
    
    
    
    

    private async Task<bool> DeactivateTripAsync(TransportType transportType, int tripId, int marshalId)
    {
        switch (transportType)
        {
            case TransportType.Bus:
                var busTrip = await _context.BusTrips.FindAsync(tripId);
                if (busTrip == null || busTrip.MarshalId != marshalId)  
                    return false;
                busTrip.Status = TripStatus.Cancelled;
                break;
            case TransportType.Railway:
                var railwayTrip = await _context.RailwayTrips.FindAsync(tripId);
                if (railwayTrip == null || railwayTrip.MarshalId != marshalId) 
                    return false;
                railwayTrip.Status = TripStatus.Cancelled;
                break;
            case TransportType.Taxi:
                var taxiTrip = await _context.TaxiTrips.FindAsync(tripId);
                if (taxiTrip == null || taxiTrip.MarshalId != marshalId) 
                    return false;
                taxiTrip.Status = TripStatus.Cancelled;
                break;
            default:
                return false;
        }

        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> CommenceTripAsync(TransportType transportType, int tripId, int marshalId)
{
    // Find the trip
    object trip;
    switch (transportType)
    {
        case TransportType.Bus:
            trip = await _context.BusTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId);
            break;
        case TransportType.Railway:
            trip = await _context.RailwayTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId);
            break;
        case TransportType.Taxi:
            trip = await _context.TaxiTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId);
            break;
        default:
            return false;
    }

    if (trip == null)
        return false;

    // Check status
    var currentStatus = trip switch
    {
        BusTrip b => b.Status,
        RailwayTrip r => r.Status,
        TaxiTrip t => t.Status,
        _ => throw new InvalidOperationException()
    };

    if (currentStatus != TripStatus.Inactive)
        return false; // already commenced, completed, or cancelled
    
    var departure = trip switch
    {
        BusTrip b => b.DepartureTime,
        RailwayTrip r => r.DepartureTime,
        TaxiTrip t => t.PickupTime,
        _ => DateTime.MaxValue
    };

    if (DateTime.Now > departure)
    {
        _logger.LogWarning("Marshal {MarshalId} tried to commence {Type} trip {TripId} which has already departed",
            marshalId, transportType, tripId);
        return false;
    }
    
    // Set status and CommencedAt
    var now = DateTime.Now;
    switch (trip)
    {
        case BusTrip b:
            b.Status = TripStatus.InProgress;
            b.CommencedAt = now;
            break;
        case RailwayTrip r:
            r.Status = TripStatus.InProgress;
            r.CommencedAt = now;
            break;
        case TaxiTrip t:
            t.Status = TripStatus.InProgress;
            t.CommencedAt = now;
            break;
    }
    // Expire unvalidated tickets for this trip
    var bookings = await GetActiveBookingsAsync(transportType, tripId);
    foreach (var booking in bookings)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.BookingId == booking.Id);
        if (ticket is { Status: TicketStatus.Issued })
        {
            ticket.Status = TicketStatus.Expired;
        }
    }
    await _context.SaveChangesAsync();
    _logger.LogInformation("Marshal {MarshalId} commenced {Type} trip {TripId}; unvalidated tickets expired", marshalId, transportType, tripId);
    return true;
}
    
    // Add this method anywhere in the class (e.g., right after CommenceTripAsync):

    public async Task<bool> EndTripAsync(TransportType transportType, int tripId, int marshalId)
    {
        object? trip = transportType switch
        {
            TransportType.Bus => await _context.BusTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId),
            TransportType.Railway => await _context.RailwayTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId),
            TransportType.Taxi => await _context.TaxiTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId),
            _ => null
        };

        if (trip == null) return false;

        var currentStatus = trip switch
        {
            BusTrip b => b.Status,
            RailwayTrip r => r.Status,
            TaxiTrip t => t.Status,
            _ => throw new InvalidOperationException()
        };

        if (currentStatus != TripStatus.InProgress)
            return false; // only in‑progress trips can be ended

        switch (trip)
        {
            case BusTrip b: b.Status = TripStatus.Completed; break;
            case RailwayTrip r: r.Status = TripStatus.Completed; break;
            case TaxiTrip t: t.Status = TripStatus.Completed; break;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Marshal {MarshalId} ended {Type} trip {TripId}", marshalId, transportType, tripId);
        return true;
    }

    private async Task<List<Booking>> GetActiveBookingsAsync(TransportType transportType, int tripId)
    {
        var query = _context.Bookings
            .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending));

        query = transportType switch
        {
            TransportType.Bus => query.Where(b => b.BusTripId == tripId),
            TransportType.Railway => query.Where(b => b.RailwayTripId == tripId),
            TransportType.Taxi => query.Where(b => b.TaxiTripId == tripId),
            _ => query.Where(b => false) // no matches for invalid type
        };

        return await query.ToListAsync();
    }
    
    
    
}
