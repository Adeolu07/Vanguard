using Tripfinity.Models.Tables;
using Tripfinity.Utilities;

namespace Tripfinity.Interfaces;

public interface ITripListingService
{
    Task<PaginatedList<BusTrip>> GetActiveBusTripsAsync(int page, int pageSize);
    Task<PaginatedList<RailwayTrip>> GetActiveRailwayTripsAsync(int page, int pageSize);
    Task<PaginatedList<TaxiTrip>> GetActiveTaxiTripsAsync(int page, int pageSize);
}