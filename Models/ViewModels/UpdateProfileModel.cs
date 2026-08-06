using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.ViewModels;

public class UpdateProfileModel
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName  { get; set; } = string.Empty;
    [Required, Phone] public string PhoneNumber { get; set; } = string.Empty;
}