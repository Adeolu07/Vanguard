namespace _Tripfinity.Models.Tables;

public class WalletTransaction
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = Guid.NewGuid().ToString("N");
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "CREDIT", "DEBIT", "REFUND"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Wallet Wallet { get; set; } = null!;
}