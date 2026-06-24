namespace _Tripfinity.Models.Data.Requests;

public class CreditWalletRequest
{
    public required decimal Amount { get; set; }
    public string? CustomerId { get; set; }
    public string? Description { get; set; }
    public string? TraceId { get; set; }
}