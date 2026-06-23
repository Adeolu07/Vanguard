using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Data.Requests;

public class CancelTripRequest
{
    [Required] public string TransportType { get; set; } = string.Empty; // Bus, Railway, Taxi
    [Required] public int TripId { get; set; }
    [Required] public string Reason { get; set; } = string.Empty;
}
