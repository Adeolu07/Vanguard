using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponse> SignUpAsync(string email, string password, string firstName, string lastName)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            return new AuthResponse
            {
                Success = false,
                Message = "Email already exists"
            };

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = email,
            PasswordHash = hasher.HashPassword(null, password),
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.Now,
            IsActive = true,
            Role = "Passenger"
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return new AuthResponse
        {
            Success = true,
            Message = "Account creation successful",
            User = user
        };
    }

    public async Task<AuthResponse?> SignInAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return new AuthResponse
            {
                Success = false,
                Message = "User not found"
            };

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result != PasswordVerificationResult.Success)
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };

        return new AuthResponse
        {
            Success = true,
            Message = "Successful Login",
            User = user
        };
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public void SetUserSession(HttpContext httpContext, User user)
    {
        httpContext.Session.SetString("UserEmail", user.Email);
        httpContext.Session.SetString("Username", $"{user.FirstName}.{user.LastName}");
        httpContext.Session.SetInt32("UserId", user.Id);
    }

    public void ClearUserSession(HttpContext httpContext)
    {
        httpContext.Session.Clear();
    }

    public User? GetCurrentUser(HttpContext httpContext)
    {
        var userId = httpContext.Session.GetInt32("UserId");
        if (userId == null)
            return null;

        return _context.Users.Find(userId.Value);
    }
}