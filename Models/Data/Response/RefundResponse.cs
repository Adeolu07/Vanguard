namespace _Tripfinity.Models.Data.Response;

public class RefundResponse
{
    public required ResponseHeader ResponseHeader { get; set; }
    public decimal? Amount { get; set; }
    
}