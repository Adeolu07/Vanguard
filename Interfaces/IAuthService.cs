using _Tripfinity.Models;
using _Tripfinity.Models.Data;

namespace _Tripfinity.Interfaces;

public interface IAuthService
{
    
    Task<AuthResponse> SignUpAsync(string email, string password, string firstName, string lastName, string phoneNumber, string role);

    Task<AuthResponse?> SignInAsync(string email, string password, string role);

    Task<bool> EmailExistsAsync(string email);

    void SetUserSession(HttpContext httpContext, User user);

    void ClearUserSession(HttpContext httpContext);

    User? GetCurrentUser(HttpContext httpContext);

    Task<bool> ConfirmationEmailAsync(string userId, string token);
}
