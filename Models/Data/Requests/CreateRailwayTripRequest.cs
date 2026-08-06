using System.ComponentModel.DataAnnotations;
using _Tripfinity.Utilities;

namespace _Tripfinity.Models.Data.Requests;

public class CreateRailwayTripRequest
{
    [Required, MaxLength(50)] public string From { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Destination { get; set; } = string.Empty;
    [Required, Range(100,50000)] public decimal Price { get; set; }
    [Required, Range(1,50)] public int TotalSeats { get; set; }
    [Required, Range(1,50)] public int AvailableSeats { get; set; }
    [Required, FutureTime(60)] public DateTime DepartureTime { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (AvailableSeats > TotalSeats)
        
            yield return new ValidationResult(
                "Available seats cannot exceed total seats.", [nameof(AvailableSeats)]);
    }
}