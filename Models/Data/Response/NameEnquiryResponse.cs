namespace _Tripfinity.Models.Data.Response;

public class NameEnquiryResponse
{
    public required string SessionId { get; set; }
    public required string EnquirerAccountNumber { get; set; }
    public required string DestinationInstitutionCode { get; set; }
    public required string AccountNumber{ get; set; }
    public required string AccountName{ get; set; }
    public required string ResponseCode { get; set; }
    public required string ResponseMessage { get; set; }
    public string? Bvn { get; set; }
    public string? KycLevel { get; set; }
    public required string AccountType { get; set; }
}

public class NameEnquiryWrapper
{
    public NameEnquiryResponse? Data { get; set; } = default;
    public bool Status { get; set; }
    public string Message { get; set; } = "";
}