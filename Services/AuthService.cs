using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;

    public AuthService(AppDbContext context, IWalletService walletService)
    {
        _context = context;
        _walletService = walletService;
    }

    public async Task<AuthResponse> SignUpAsync(string email, string password, string firstName, string lastName, string phoneNumber, string role)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            return new AuthResponse { Success = false, Message = "Email already exists" };

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.Now,
            IsActive = false,
            Role = role,
            EmailConfirmationToken = Guid.NewGuid().ToString(),
            ConfirmationTokenExpiry = DateTime.Now.AddDays(1)
        };

        user.PasswordHash = hasher.HashPassword(user, password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = $"{role} account created. Please confirm your email",
            User = user
        };
    }

    public async Task<AuthResponse?> SignInAsync(string email, string password, string role)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == role);

        if (user == null)
            return new AuthResponse { Success = false, Message = $"{role} not found" };

        if (!user.IsEmailConfirmed)
            return new AuthResponse { Success = false, Message = "Please confirm your Email" };

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result != PasswordVerificationResult.Success)
            return new AuthResponse { Success = false, Message = "Invalid credentials" };

        return new AuthResponse { Success = true, Message = "Successful Login", User = user };
    }

    public async Task<bool> EmailExistsAsync(string email) => await _context.Users.AnyAsync(u => u.Email == email);

    public void SetUserSession(HttpContext httpContext, User user) => httpContext.Session.SetInt32("userId", user.Id);

    public void ClearUserSession(HttpContext httpContext) => httpContext.Session.Clear();

    public User? GetCurrentUser(HttpContext httpContext)
    {
        var userId = httpContext.Session.GetInt32("userId");
        return userId == null ? null : _context.Users.Find(userId);
    }

    public async Task<bool> ConfirmationEmailAsync(string userId, string token)
    {
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user == null || user.EmailConfirmationToken != token || user.ConfirmationTokenExpiry < DateTime.Now)
            return false;

        user.IsEmailConfirmed = true;
        user.IsActive = true;
        user.EmailConfirmationToken = null;
        user.ConfirmationTokenExpiry = null;

        var createWalletRequest = new CreateWalletRequest
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
        };

        var response = await _walletService.CreateWalletAsync(createWalletRequest);
        user.UserWalletId = response.AccountDetails.CustomerId;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
