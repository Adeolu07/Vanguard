namespace _Tripfinity.Models.Data.Response;

public class NameEnquiryResponse
{
    public string? SessionId { get; set; }
    public string? EnquirerAccountNumber { get; set; }
    public string? DestinationInstitutionCode { get; set; }
    public string? AccountNumber{ get; set; }
    public string? AccountName{ get; set; }
    public required string ResponseCode { get; set; }
    public required string ResponseMessage { get; set; }
    public string? Bvn { get; set; }
    public string? KycLevel { get; set; }
    public string? AccountType { get; set; }
}

public class NameEnquiryWrapper
{
    public NameEnquiryResponse Data { get; set; } = default!;
    public bool Status { get; set; }
    public string Message { get; set; } = "";
}