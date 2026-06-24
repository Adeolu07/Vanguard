using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Identity.Data;

namespace _Tripfinity.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SignUpAsync(RegisterViewModel model);

    Task<AuthResponse> RegisterMarshalAsync(MarshalRegisterRequest request);

    Task<AuthResponse?> SignInAsync(string email, string password);

    Task<bool> EmailExistsAsync(string email);

    void SetUserSession(HttpContext httpContext, User user);

    void ClearUserSession(HttpContext httpContext);

    User? GetCurrentUser(HttpContext httpContext);
    public Task<bool> ConfirmationEmailAsync(int userId, string token);
    public Task<AuthResponse> ForgotPasswordAsync(string email);
    public Task<bool> ValidatePasswordResetTokenAsync(string email, string token);
    public Task<AuthResponse> ResetPasswordAsync(string email, string token, string newPassword);
    // public Task<IActionResult> ResetPasswordAsync(HttpContext httpContext);

}