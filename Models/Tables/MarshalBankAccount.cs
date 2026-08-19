namespace _Tripfinity.Models.Tables;

public class MarshalBankAccount
{
    public int MarshalId { get; set; }              // PK + FK → User.Id
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User Marshal { get; set; } = null!;
    
}