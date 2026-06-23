using _Tripfinity.Models;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Interfaces;

public interface ITripService
{
    Task<BusTrip> CreateBusTripAsync(BusTrip trip);
    Task<RailwayTrip> CreateRailwayTripAsync(RailwayTrip trip);
    Task<TaxiTrip> CreateTaxiTripAsync(TaxiTrip trip);

    // Deactivates a trip and cancels/refunds any active bookings on it.
    // Returns false if the trip was not found.
    Task<bool> CancelTripAsync(string transportType, int tripId, int marshalId, string reason);
}
