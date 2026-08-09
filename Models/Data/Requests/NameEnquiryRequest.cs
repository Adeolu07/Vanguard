namespace _Tripfinity.Models.Data.Requests;

public class NameEnquiryRequest
{
    public required string SessionId { get; set; }
    public required string SenderAccountNumber { get; set; }
    public required string DestinationInstitutionCode { get; set; }
    public required string DestinationAccountNumber { get; set; }
    
}