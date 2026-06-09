using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> SignUpAsync(string email, string password, string firstName, string lastName, string role)
    {
        // Prevent duplicate registration with same email, name, and role
        var exists = await _context.Users.AnyAsync(u =>
            u.Email == email &&
            u.FirstName == firstName &&
            u.LastName == lastName &&
            u.Role == role);

        if (exists) return null; // reject registration

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Password = hashedPassword,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> SignInAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.EmailConfirmed);
        if (user == null) return null;

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            return null;

        // Verify password hash
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 3)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(30); 
                user.FailedLoginAttempts = 0; // reset counter after lock
            }
            await _context.SaveChangesAsync();
            return null;
        }

        // Reset attempts on success
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        await _context.SaveChangesAsync();
        return user;
    }


    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        var token = Guid.NewGuid().ToString();
        user.EmailConfirmationToken = token;
        user.TokenGeneratedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        if (!int.TryParse(userId, out int id))
            return false;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;

        // Compare token safely
        if (user.EmailConfirmationToken != token) return false;

        // Check expiry (24 hours)
        if (user.TokenGeneratedAt == null || user.TokenGeneratedAt < DateTime.UtcNow.AddHours(-24))
            return false;

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.TokenGeneratedAt = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        var token = Guid.NewGuid().ToString();
        user.PasswordResetToken = token;
        user.PasswordResetTokenGeneratedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.PasswordResetToken != token) return false;

        if (user.PasswordResetTokenGeneratedAt == null || user.PasswordResetTokenGeneratedAt < DateTime.UtcNow.AddHours(-24))
            return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword); // hash new password
        user.PasswordResetToken = null;
        user.PasswordResetTokenGeneratedAt = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    // method for resending confirmation email
    public async Task<KeyValuePair<string, string>> ResendConfirmationAsync(
        string email,
        IEmailService emailService,
        IUrlHelper url,
        string scheme)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return new KeyValuePair<string, string>("Error", "Email not found.");

        if (user.EmailConfirmed)
            return new KeyValuePair<string, string>("Success", "Your email is already confirmed. Please sign in.");

        var token = await GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token }, scheme);

        await emailService.SendEmailAsync(user.Email, "Confirm your Tripfinity account",
            $"Click here to confirm your email: {confirmationLink}");

        return new KeyValuePair<string, string>("Success", "A new confirmation link has been sent to your email.");
    }

    public void SetUserSession(HttpContext httpContext, User user)
    {
        httpContext.Session.SetString("UserEmail", user.Email);
        httpContext.Session.SetString("Username", $"{user.FirstName} {user.LastName}");
    }

    public void ClearUserSession(HttpContext httpContext)
    {
        httpContext.Session.Clear();
    }
}
