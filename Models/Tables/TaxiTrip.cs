using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Tables;

public class TaxiTrip
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string PickupLocation { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string DropoffLocation { get; set; } = string.Empty;

    [Required] [Range(500, 50000)] public decimal Price { get; set; }

    [Required] [Range(1, 4)] public int MaxPassengers { get; set; } = 4;

    [Required] public DateTime PickupTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    [MaxLength(50)] public string? VehicleType { get; set; } // Sedan, SUV, Luxury
}