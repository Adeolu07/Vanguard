namespace _Tripfinity.Models.Data.Response;

public class CreateWalletResponse
{
    public required ResponseHeader ResponseHeader { get; set; }
    public required AccountDetails AccountDetails { get; set; }
    
}

public class AccountDetails
{
    public required string AccountNumber { get; set; }
    public required string CustomerId{ get; set; }
    public required string CustomerAlias { get; set; }
    public required string BankName { get; set; }
    public required string BankCode { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Bvn { get; set; }
}