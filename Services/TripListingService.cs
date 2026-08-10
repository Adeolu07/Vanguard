using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Enums;
using _Tripfinity.Models.Tables;
using _Tripfinity.Views;
using Microsoft.EntityFrameworkCore; // needed for PaginatedList if it's defined elsewhere; adjust if it's in your own namespace

namespace _Tripfinity.Services;

public class TripListingService : ITripListingService
{
    private readonly AppDbContext _context;

    public TripListingService(AppDbContext context) => _context = context;

    public async Task<PaginatedList<BusTrip>> GetActiveBusTripsAsync(int page, int pageSize)
    {
        var query = _context.BusTrips
            .Where(t => t.Status == TripStatus.Inactive && t.DepartureTime > DateTime.Now)
            .OrderBy(t => t.DepartureTime);
        return await PaginatedList<BusTrip>.CreateAsync(query, page, pageSize);
    }

    public async Task<PaginatedList<RailwayTrip>> GetActiveRailwayTripsAsync(int page, int pageSize)
    {
        var query = _context.RailwayTrips
            .Where(t => t.Status == TripStatus.Inactive && t.DepartureTime > DateTime.Now)
            .OrderBy(t => t.DepartureTime);
        return await PaginatedList<RailwayTrip>.CreateAsync(query, page, pageSize);
    }

    public async Task<PaginatedList<TaxiTrip>> GetActiveTaxiTripsAsync(int page, int pageSize)
    {
        var query = _context.TaxiTrips
            .Where(t => t.Status == TripStatus.Inactive && t.PickupTime > DateTime.Now)
            .OrderBy(t => t.PickupTime);
        return await PaginatedList<TaxiTrip>.CreateAsync(query, page, pageSize);
    }
}