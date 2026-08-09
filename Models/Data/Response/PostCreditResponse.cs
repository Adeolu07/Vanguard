namespace _Tripfinity.Models.Data.Response;

public class PostCreditResponse
{
    public string? SessionId { get; set; }
    public string? SourceSessionId { get; set; }
    public string? PaymentReference { get; set; }
    public string? SenderAccountNumber { get; set; }
    public string? DestinationInstitutionCode { get; set; }
    public string? ReceiverAccountNumber { get; set; }
    public decimal Amount { get; set; }
    public string? Channel { get; set; }
    public string? Group { get; set; }
    public string? Sector { get; set; }
    public string? Narration { get; set; }
    public required string ResponseCode { get; set; }
    public required string ResponseMessage { get; set; }
    public string? ReceiverAccountName { get; set; }
    public DateTime? TransactionDate { get; set; }

}

 public class PostCreditWrapper
 { 
     public PostCreditResponse Data { get; set; } = default!;
     public bool Status { get; set; }
     public string Message { get; set; } = "";
 }