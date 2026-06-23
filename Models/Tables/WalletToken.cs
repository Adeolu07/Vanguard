namespace _Tripfinity.Models.Tables;

public class WalletToken
{
    public int Id { get; set; } 
    public  string Token{ get; set; }
    public DateTime ExpiryDate { get; set; }

}