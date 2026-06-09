using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models;

public class RailwayBooking
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int RailwayTripId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime BookingDate { get; set; } = DateTime.Now;

    public DateTime? CancelledAt { get; set; }

    [MaxLength(255)]
    public string? CancellationReason { get; set; }

    public User? User { get; set; }
    public RailwayTrip? RailwayTrip { get; set; }
}