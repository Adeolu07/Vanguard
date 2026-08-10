using _Tripfinity.Models.Data.Response;

namespace _Tripfinity.Models.ViewModels;

public class MarshalWalletViewModel
{
    public string? WalletId { get; set; }
    public decimal Balance { get; set; }
    public List<TransactionDetailsList>? Transactions { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}