using _Tripfinity.Models;
using Microsoft.AspNetCore.Mvc; // ✅ Added for IUrlHelper

namespace _Tripfinity.Interfaces;

public interface IAuthService
{
    Task<User?> SignUpAsync(string email, string password, string firstName, string lastName, string role);

    Task<User?> SignInAsync(string email, string password);

    Task<bool> EmailExistsAsync(string email);

    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<bool> ConfirmEmailAsync(string userId, string token);

    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

    Task<User?> GetUserByEmailAsync(string email);

    Task<KeyValuePair<string, string>> ResendConfirmationAsync(
        string email,
        IEmailService emailService,
        IUrlHelper url,
        string scheme);

    void SetUserSession(HttpContext httpContext, User user);

    void ClearUserSession(HttpContext httpContext);
}
