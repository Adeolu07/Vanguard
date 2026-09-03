namespace _Tripfinity.Models.Data.Requests;

public class FcSinglePostReq
{
    public required string TraceId { get; set; }
    public required string Signature { get; set; }
    public int Timestamp{ get; set; }
    public required TransactionDetails TransactionDetails { get; set; }
}

public record TransactionDetails
{
    public required string CreditAccount { get; set; }
    public required string CreditAccountName { get; set; }
    public required string CreditBankCode { get; set; }
    public required string Narration { get; set; }
    public decimal Amount { get; set; }
}