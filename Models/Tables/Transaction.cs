using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Tables;

public class Transaction
{
    [Key]
    public int TransactionId { get; set; }

    [Required]
    public int UserId { get; set; }     

    [Required, MaxLength(12)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [Range(0, 100000)]
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "NGN";

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ExternalReference { get; set; }   // CoralPay transaction ID

    [MaxLength(100)]
    public string? InternalReference { get; set; }   // generated GUID, e.g. "TXN‑XXXXXXXX"

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";  // Pending, Completed, Failed, Reversed

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}