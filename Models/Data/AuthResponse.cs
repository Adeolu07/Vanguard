using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.Data;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
}