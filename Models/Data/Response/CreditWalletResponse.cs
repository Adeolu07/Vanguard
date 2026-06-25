namespace _Tripfinity.Models.Data.Response;

public class CreditWalletResponse
{
    public required ResponseHeader ResponseHeader { get; set; }
    public required decimal Amount { get; set; }
    public required decimal Balance { get; set; }
    public required string Description { get; set; }
    public required string TransactionId { get; set; }
    public required string TraceId { get; set; }
    
}