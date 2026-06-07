using _Tripfinity.Models;

namespace _Tripfinity.Interfaces;

public interface IAuthService
{
    Task<User?> SignUpAsync(string email, string password, string firstName, string lastName, string role);
    
    Task<User?> SignInAsync(string email, string password);
    
    Task<bool> EmailExistsAsync(string email);
    
    void SetUserSession(HttpContext httpContext, User user);
    
    void ClearUserSession(HttpContext httpContext);
}