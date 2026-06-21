namespace _Tripfinity.Models.Data.Requests;

public class RefundRequest
{
    public required string CustomerId{ get; set; }
    public required string Description { get; set; } = "Refund";
    public required string TransactionId{ get; set; }
}