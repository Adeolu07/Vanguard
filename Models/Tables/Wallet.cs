namespace _Tripfinity.Models.Tables;

public class Wallet
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}