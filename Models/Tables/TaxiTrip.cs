using System.ComponentModel.DataAnnotations;
using _Tripfinity.Models.Enums;

namespace _Tripfinity.Models.Tables;

public class TaxiTrip : ITrip
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string PickupLocation { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string DropoffLocation { get; set; } = string.Empty;

    [Required] [Range(500, 50000)] public decimal Price { get; set; }

    [Required] [Range(1, 4)] public int MaxPassengers { get; set; } = 4;
    public int AvailableSeats { get; set; }

    [Required] public DateTime PickupTime { get; set; }

    public DateTime CreatedAt { get; set; } 

    public TripStatus Status { get; set; } = TripStatus.Inactive;
    public DateTime? CommencedAt { get; set; }
    [MaxLength(50)] public string? VehicleType { get; set; } // Sedan, SUV, Luxury
    public int MarshalId { get; set; }
    [Required] [MaxLength(50)] public string VehicleId { get; set; } = string.Empty;
}