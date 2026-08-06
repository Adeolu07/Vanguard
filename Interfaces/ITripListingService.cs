using _Tripfinity.Models.Tables;
using _Tripfinity.Views;

namespace _Tripfinity.Interfaces;

public interface ITripListingService
{
    Task<PaginatedList<BusTrip>> GetActiveBusTripsAsync(int page, int pageSize);
    Task<PaginatedList<RailwayTrip>> GetActiveRailwayTripsAsync(int page, int pageSize);
    Task<PaginatedList<TaxiTrip>> GetActiveTaxiTripsAsync(int page, int pageSize);
}