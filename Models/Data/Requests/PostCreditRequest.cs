namespace _Tripfinity.Models.Data.Requests;

public class PostCreditRequest
{
    public required string SessionId { get; set; }
    public required string PaymentRef { get; set; }
    public required string DestinationInstitutionId { get; set; }
    public required string CreditAccount { get; set; }
    public required string CreditAccountName { get; set; }
    public required string SourceAccountId { get; set; }
    public required string SourceAccountName { get; set; }
    public required string Narration { get; set; }
    public required string Channel { get; set; }
    public required string Group { get; set; }
    public required string Sector { get; set; }
    public decimal Amount { get; set; }
    
    
    
    
    
    
    
    
}