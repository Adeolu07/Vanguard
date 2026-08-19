using System.ComponentModel.DataAnnotations;
using _Tripfinity.Models.Enums;

namespace _Tripfinity.Models.Tables;

public class BusTrip : ITrip
{
    public int Id { get; set; }

    [Required] [MaxLength(50)] public string From { get; set; } = string.Empty;

    [Required] [MaxLength(50)] public string Destination { get; set; } = string.Empty;

    [Required] [Range(100, 50000)] public decimal Price { get; set; }

    [Required] [Range(1, 50)] public int TotalSeats { get; set; }

    [Required] [Range(0, 50)] public int AvailableSeats { get; set; }

    [Required] public DateTime DepartureTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public TripStatus Status { get; set; } = TripStatus.Inactive;
    public DateTime? CommencedAt { get; set; }
    public int MarshalId { get; set; }
    [Required] [MaxLength(50)] public string VehicleId { get; set; } = string.Empty;
}