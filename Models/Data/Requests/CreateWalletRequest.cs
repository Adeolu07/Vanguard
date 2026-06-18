namespace _Tripfinity.Models.Data.Requests;

public class CreateWalletRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CustomerAlias { get; set; }
    public string Otp { get; } = "123456";
}