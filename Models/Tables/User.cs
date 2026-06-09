using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models;

public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(100, ErrorMessage = "Email too long")]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must have at least 6 characters")]
    [MaxLength(255)] 
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name too long")]
    public string FirstName { get; set; } = string.Empty;


    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name too long")]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [MaxLength(50)]
    public string? Role { get; set; }  // "Passenger", "Marshal", "Admin"
    
    public bool IsActive { get; set; }

    // 🔑 New fields for email verification
    public bool EmailConfirmed { get; set; } = false;

    [MaxLength(255)]
    public string? EmailConfirmationToken { get; set; }

    public DateTime? TokenGeneratedAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenGeneratedAt { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }


}