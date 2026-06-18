namespace _Tripfinity.Models.Data.Requests;

public class CreditWalletRequest
{
    public required decimal Amount { get; set; }
    public required string CustomerId { get; set; }
    public required string Description { get; set; }
    public required string TraceId { get; set; }
}