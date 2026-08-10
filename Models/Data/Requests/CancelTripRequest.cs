using System.ComponentModel.DataAnnotations;
using _Tripfinity.Models.Enums;


namespace _Tripfinity.Models.Data.Requests;

public class CancelTripRequest
{
    [Required] public TransportType TransportType { get; set; } // Bus, Railway, Taxi
    [Required] public int TripId { get; set; }
    [Required] public string Reason { get; set; } = string.Empty;
}
