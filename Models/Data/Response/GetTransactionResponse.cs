namespace _Tripfinity.Models.Data.Response;

public class GetTransactionResponse
{
    public ResponseHeader? ResponseHeader { get; set; }
    public TransactionDetails? TransactionDetails { get; set; }
}

public class TransactionDetails
{
    public string? TranType { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? TransactionId { get; set; }
    public string? SessionId { get; set; }
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
}