using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Data.Requests;

public class CreateRailwayTripRequest
{
    [Required, MaxLength(50)] public string From { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Destination { get; set; } = string.Empty;
    [Required, Range(100,50000)] public decimal Price { get; set; }
    [Required, Range(1,50)] public int TotalSeats { get; set; }
    [Required, Range(1,50)] public int AvailableSeats { get; set; }
    [Required, Range(typeof(DateTime), "2026","")] public DateTime DepartureTime { get; set; }
}