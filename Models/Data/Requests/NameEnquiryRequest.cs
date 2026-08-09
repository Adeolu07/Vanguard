namespace _Tripfinity.Models.Data.Requests;

public class NameEnquiryRequest
{
    public required string SessionId { get; set; }
    public required string DestinationInstitutionId { get; set; }
    public required string AccountId { get; set; }
    
}