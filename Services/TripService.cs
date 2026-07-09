using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
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

    public async Task<BusTrip> CreateBusTripAsync(BusTrip trip)
    {
        trip.CreatedAt = DateTime.Now;
        trip.IsActive = true;
        if (trip.AvailableSeats <= 0 || trip.AvailableSeats > trip.TotalSeats)
            trip.AvailableSeats = trip.TotalSeats;

        _context.BusTrips.Add(trip);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Bus trip {Id} created ({From} -> {To})", trip.Id, trip.From, trip.Destination);
        return trip;
    }

    public async Task<RailwayTrip> CreateRailwayTripAsync(RailwayTrip trip)
    {
        trip.CreatedAt = DateTime.Now;
        trip.IsActive = true;
        if (trip.AvailableSeats <= 0 || trip.AvailableSeats > trip.TotalSeats)
            trip.AvailableSeats = trip.TotalSeats;

        _context.RailwayTrips.Add(trip);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Railway trip {Id} created ({From} -> {To})", trip.Id, trip.From, trip.Destination);
        return trip;
    }

    public async Task<TaxiTrip> CreateTaxiTripAsync(TaxiTrip trip)
    {
        trip.CreatedAt = DateTime.Now;
        trip.IsActive = true;
        
        _context.TaxiTrips.Add(trip);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Taxi trip {Id} created ({From} -> {To})", trip.Id, trip.PickupLocation, trip.DropoffLocation);
        return trip;
    }

    public async Task<bool> CancelTripAsync(TransportType transportType, int tripId, int marshalId, string reason)
    {
        var found = await DeactivateTripAsync(transportType, tripId);
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

    private async Task<bool> DeactivateTripAsync(TransportType transportType, int tripId)
    {
        switch (transportType)
        {
            case TransportType.Bus:
                var bus = await _context.BusTrips.FindAsync(tripId);
                if (bus == null) 
                    return false;
                bus.IsActive = false;
                break;
            case TransportType.Railway:
                var rail = await _context.RailwayTrips.FindAsync(tripId);
                if (rail == null) 
                    return false;
                rail.IsActive = false;
                break;
            case TransportType.Taxi:
                var taxi = await _context.TaxiTrips.FindAsync(tripId);
                if (taxi == null) 
                    return false;
                taxi.IsActive = false;
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
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending);
        
        return await query.ToListAsync();
    }
    
}
