using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Tables;

public class WalletTransaction
{
    public int Id { get; set; }

    [Required]
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty; // Credit | Debit | Refund

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? TraceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsReversed { get; set; } = false;
}