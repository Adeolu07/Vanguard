using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Data.Requests;

public class MarshalRegisterRequest
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] [MinLength(6)] public string Password { get; set; } = string.Empty;
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string VehicleType { get; set; } = string.Empty; // Bus, Railway, Taxi
    [Required] public string LicenseId { get; set; } = string.Empty;
}
