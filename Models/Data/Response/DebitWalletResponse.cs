namespace _Tripfinity.Models.Data.Response;

public class DebitWalletResponse
{
    public ResponseHeader? ResponseHeader { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    public string? TransactionId { get; set; }
    public string? TraceId { get; set; }
}