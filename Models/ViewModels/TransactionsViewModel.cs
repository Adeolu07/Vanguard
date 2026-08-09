using _Tripfinity.Models.Data.Response;

namespace _Tripfinity.Models.ViewModels;

public class TransactionsViewModel
{
    public string? WalletId { get; set; }
    public decimal Balance { get; set; }
    public List<TransactionDetailsList>? Transactions { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}