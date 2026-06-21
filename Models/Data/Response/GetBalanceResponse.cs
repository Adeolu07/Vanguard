namespace _Tripfinity.Models.Data.Response;

public class GetBalanceResponse
{
    public required ResponseHeader ResponseHeader { get; set; }
    public required decimal Balance { get; set; }
}