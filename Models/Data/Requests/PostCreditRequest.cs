namespace _Tripfinity.Models.Data.Requests;

public class PostCreditRequest
{
    public required string SessionId { get; set; }
    public required string PaymentReference { get; set; }
    public required string SenderAccountNumber { get; set; }
    public required string DestinationInstitutionCode { get; set; }
    public required string DestinationAccountNumber { get; set; }
    public required string DestinationAccountName { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "01";
    public string Channel { get; set; } = "Others";
    public string Group { get; set; } = "Cooperate";
    public string Sector { get; set; } = "Banking";
    public string Narration { get; set; } = "";
}