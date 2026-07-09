using System.ComponentModel.DataAnnotations;
using _Tripfinity.Services;

namespace _Tripfinity.Models.Tables;

public class Ticket
{
    public int Id { get; set; }

    // Human/QR-facing reference, e.g. TKT-XXXXXXXX
    [Required] [MaxLength(50)] public string TicketReference { get; set; } = string.Empty;

    [Required] public int BookingId { get; set; }

    // Passenger (User.Id) the ticket was issued to
    [Required] public int PassengerId { get; set; }

    // Vehicle assigned to the trip (Marshal's VehicleId), may be null until a marshal is assigned
    [MaxLength(50)] public string? VehicleId { get; set; }

    [Required] [MaxLength(20)] public TransportType TransportType { get; set; } // Bus, Taxi, Railway

    [Required] public DateTime TripTime { get; set; }

    [Required] public decimal Fare { get; set; }

    [Required] [MaxLength(20)] public TicketStatus Status { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.Now;

    public DateTime? ValidatedAt { get; set; }

    public int? ValidatedByMarshalId { get; set; }

    // NEW: store QR image as Base64
    public string QrCodeBase64 { get; set; } = string.Empty;

    // Navigation
    public Booking? Booking { get; set; }
}
