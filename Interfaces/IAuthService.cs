using _Tripfinity.Models;
using _Tripfinity.Models.Data;

namespace _Tripfinity.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SignUpAsync(string email, string password, string firstName, string lastName);

    Task<AuthResponse?> SignInAsync(string email, string password);

    Task<bool> EmailExistsAsync(string email);

    void SetUserSession(HttpContext httpContext, User user);

    void ClearUserSession(HttpContext httpContext);

    User? GetCurrentUser(HttpContext httpContext);
}