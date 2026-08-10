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
                busTrip.IsActive = false;
                break;
            case TransportType.Railway:
                var railwayTrip = await _context.RailwayTrips.FindAsync(tripId);
                if (railwayTrip == null || railwayTrip.MarshalId != marshalId) 
                    return false;
                railwayTrip.IsActive = false;
                break;
            case TransportType.Taxi:
                var taxiTrip = await _context.TaxiTrips.FindAsync(tripId);
                if (taxiTrip == null || taxiTrip.MarshalId != marshalId) 
                    return false;
                taxiTrip.IsActive = false;
                break;
            default:
                return false;
        }

        await _context.SaveChangesAsync();
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
