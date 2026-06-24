using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.ViewModels;

public class ResetPasswordViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}