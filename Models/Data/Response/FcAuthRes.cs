namespace _Tripfinity.Models.Data.Response;

public class FcAuthRes
{
    public required ResponseHeader ResponseHeader { get; set; }
    public string? Token { get; set; }
    public string? Key { get; set; }
    public string? ExpiryDate { get; set; }
}