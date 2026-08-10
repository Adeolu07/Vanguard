using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Enums;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Interfaces;

public interface ITripService
{
    Task<BusTrip> CreateBusTripAsync(CreateBusTripRequest request, int marshalId, string vehicleId);
    Task<RailwayTrip> CreateRailwayTripAsync(CreateRailwayTripRequest request, int marshalId, string vehicleId);
    Task<TaxiTrip> CreateTaxiTripAsync(CreateTaxiTripRequest request, int marshalId, string vehicleId);

    Task<bool> CancelTripAsync(TransportType transportType, int tripId, int marshalId, string reason);
    Task<bool> CommenceTripAsync(TransportType transportType, int tripId, int marshalId);
    // Add inside the interface, after CommenceTripAsync:
    Task<bool> EndTripAsync(TransportType transportType, int tripId, int marshalId);
}