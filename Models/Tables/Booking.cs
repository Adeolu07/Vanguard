using System.ComponentModel.DataAnnotations;
using _Tripfinity.Services;
namespace _Tripfinity.Models.Tables;

public class Booking
{
    public int Id { get; set; }

    [Required] public int UserId { get; set; }

    // Nullable foreign keys for different transport types
    public int? BusTripId { get; set; }
    public int? TaxiTripId { get; set; }
    public int? RailwayTripId { get; set; }

    [Required] [MaxLength(20)] public TransportType TransportType { get; set; } // "Bus", "Taxi", "Railway"

    [Required] public int NumberOfSeats { get; set; } = 1;

    [Required] public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(20)]
    public BookingStatus Status { get; set; }

    public DateTime BookingDate { get; set; }

    public DateTime? CancelledAt { get; set; }

    [MaxLength(255)] public string? CancellationReason { get; set; }

    // Payment tracking (CoralPay wallet)
    [MaxLength(100)] public string? PaymentTransactionId { get; set; }
    [MaxLength(100)] public string? PaymentTraceId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public BusTrip? BusTrip { get; set; }
    public TaxiTrip? TaxiTrip { get; set; }
    public RailwayTrip? RailwayTrip { get; set; }
}