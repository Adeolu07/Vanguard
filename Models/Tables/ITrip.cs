using _Tripfinity.Models.Enums;

namespace _Tripfinity.Models.Tables;

/// <summary>
/// Common surface shared by BusTrip, RailwayTrip and TaxiTrip.
/// Only mapped properties are exposed so EF Core can translate queries on them.
/// </summary>
public interface ITrip
{
    int Id { get; }
    decimal Price { get; }
    int AvailableSeats { get; set; }
    TripStatus Status { get; set; }
    int MarshalId { get; set; }
    string VehicleId { get; set; }
    DateTime CreatedAt { get; }
}