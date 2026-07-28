using System.ComponentModel.DataAnnotations;
using NuGet.Protocol.Plugins;

namespace _Tripfinity.Models.Tables;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(100, ErrorMessage = "Email too long")]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must have at least 6 characters")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name too long")]
    public required string FirstName { get; set; } 


    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name too long")]
    public required string LastName { get; set; }

    [MaxLength(20)] [Phone] public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    [MaxLength(50)] public string? Role { get; set; } // "Passenger", "Marshal", "Admin"

    public bool IsActive { get; set; }
    public bool IsEmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
    public DateTime? ConfirmationTokenExpiry { get; set; }
    
    public DateTime? EmailConfirmationSentAt { get; set; }

    public string? UserWalletId { get; set; } = null;
    public string? PasswordResetToken { get; set; } 
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Marshal-specific fields (only populated when Role == "Marshal")
    [MaxLength(50)] public string? VehicleType { get; set; } 
    [MaxLength(50)] public string? LicenseId { get; set; }
    [MaxLength(50)] public string? VehicleId { get; set; }
    public ICollection<BusTrip> BusTrips { get; set; } = new List<BusTrip>();
    public ICollection<TaxiTrip> TaxiTrips { get; set; } = new List<TaxiTrip>();
    public ICollection<RailwayTrip> RailwayTrips { get; set; } = new List<RailwayTrip>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}