namespace _Tripfinity.Models.Data.Response;

public class CreateWalletResponse
{
    public ResponseHeader? ResponseHeader { get; set; }
    public AccountDetails? AccountDetails { get; set; }
}

public class AccountDetails
{
    public string? AccountNumber { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerAlias { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bvn { get; set; }
}