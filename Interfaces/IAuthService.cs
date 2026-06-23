using _Tripfinity.Models;
using _Tripfinity.Models.Data;

namespace _Tripfinity.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SignUpAsync(string email, string password, string firstName, string lastName);

    Task<AuthResponse> RegisterMarshalAsync(string email, string password, string firstName, string lastName,
        string vehicleType, string licenseId);

    Task<AuthResponse?> SignInAsync(string email, string password);

    Task<bool> EmailExistsAsync(string email);

    void SetUserSession(HttpContext httpContext, User user);

    void ClearUserSession(HttpContext httpContext);

    User? GetCurrentUser(HttpContext httpContext);
    public Task<bool> ConfirmationEmailAsync(int userId, string token);
    public Task<AuthResponse> ForgotPasswordAsync(string email);
    // public Task<IActionResult> ResetPasswordAsync(HttpContext httpContext);

}