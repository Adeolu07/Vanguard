namespace _Tripfinity.Models.Data.Response;

public class AuthenticationResponse
{
    public required ResponseHeader  ResponseHeader { get; set; }
    public string? Token { get; set; }
    public required string ExpiryDate { get; set; }
}

public record ResponseHeader
{
    public required string ResponseMessage { get; set; }
    public required string ResponseCode { get; set; }
}