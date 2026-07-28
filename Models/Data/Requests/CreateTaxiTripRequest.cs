using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Data.Requests;

public class CreateTaxiTripRequest
{
    [Required, MaxLength(50)] public string PickupLocation { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string DropoffLocation { get; set; } = string.Empty;
    [Required, Range(100,50000)] public decimal Price { get; set; }
    [Required, Range(1,50)] public int NumberOfPassengers { get; set; }
    [Required] public DateTime PickupTime { get; set; }
}