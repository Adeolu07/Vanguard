namespace _Tripfinity.Models.Data.Response;

public class GetBalanceResponse
{
    public ResponseHeader? ResponseHeader { get; set; }
    public decimal Balance { get; set; }
}