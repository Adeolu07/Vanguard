namespace _Tripfinity.Models.Data.Response;

public class AuthenticationResponse
{
    public ResponseHeader? ResponseHeader { get; set; }
    public string? Token { get; set; }
    public string? ExpiryDate { get; set; }
}

public class ResponseHeader
{
    public string? ResponseMessage { get; set; }
    public string? ResponseCode { get; set; }
}