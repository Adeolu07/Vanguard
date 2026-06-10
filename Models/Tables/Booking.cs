using System.ComponentModel.DataAnnotations;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models;

public class Booking
{
    public int Id { get; set; }

    [Required] public int UserId { get; set; }

    // Nullable foreign keys for different transport types
    public int? BusTripId { get; set; }
    public int? TaxiTripId { get; set; }
    public int? RailwayTripId { get; set; }

    [Required] [MaxLength(20)] public string TransportType { get; set; } = string.Empty; // "Bus", "Taxi", "Railway"

    [Required] public int NumberOfSeats { get; set; } = 1;

    [Required] public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

    public DateTime BookingDate { get; set; } = DateTime.Now;

    public DateTime? CancelledAt { get; set; }

    [MaxLength(255)] public string? CancellationReason { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public BusTrip? BusTrip { get; set; }
    public TaxiTrip? TaxiTrip { get; set; }
    public RailwayTrip? RailwayTrip { get; set; }
}