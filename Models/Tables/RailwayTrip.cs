using System.ComponentModel.DataAnnotations;
using _Tripfinity.Models.Enums;

namespace _Tripfinity.Models.Tables;

public class RailwayTrip
{
    public int Id { get; set; }

    [Required] [MaxLength(50)] public string From { get; set; } = string.Empty;

    [Required] [MaxLength(50)] public string Destination { get; set; } = string.Empty;

    [Required] [Range(100, 500000)] public decimal Price { get; set; }

    [Required] [Range(1, 500)] public int TotalSeats { get; set; }

    [Required] [Range(0, 500)] public int AvailableSeats { get; set; }

    [Required] public DateTime DepartureTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public TripStatus Status { get; set; } = TripStatus.Inactive;
    public DateTime? CommencedAt { get; set; }

    [Required] [MaxLength(20)] public string TrainClass { get; set; } = "Regular"; // Regular | Business | First

    [Required]
    [MaxLength(50)]
    public string Route { get; set; } = string.Empty; // e.g. AKTS | Lagos-Ibadan | Warri-Itakpe
    public int MarshalId { get; set; }
    [Required] [MaxLength(50)] public string VehicleId { get; set; } = string.Empty;

}