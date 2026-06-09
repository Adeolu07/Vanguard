using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models;

public class RailwayTrip
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string From { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    [Range(100, 500000)]
    public decimal Price { get; set; }

    [Required]
    [Range(1, 500)]
    public int TotalSeats { get; set; }

    [Required]
    [Range(0, 500)]
    public int AvailableSeats { get; set; }

    [Required]
    public DateTime DepartureTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    [Required]
    [MaxLength(20)]
    public string TrainClass { get; set; } = "Regular"; // Regular | Business | First

    [Required]
    [MaxLength(50)]
    public string Route { get; set; } = string.Empty; // e.g. AKTS | Lagos-Ibadan | Warri-Itakpe
}