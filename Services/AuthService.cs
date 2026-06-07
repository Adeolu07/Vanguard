using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
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
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User?> SignInAsync(string email, string password)
    {
        return await _context.Users.FirstOrDefaultAsync(u => 
            u.Email == email && u.Password == password);
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
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