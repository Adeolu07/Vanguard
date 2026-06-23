using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext context,  IWalletService walletService,  IEmailService emailService)
    {
        _context = context;
        _walletService = walletService;
        _emailService = emailService;
    }

    public async Task<AuthResponse> SignUpAsync(string email, string password, string firstName, string lastName)
    {
        try
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
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.Now,
                IsActive = false,
                Role = "Passenger",
                EmailConfirmationToken = Guid.NewGuid().ToString(),
                ConfirmationTokenExpiry = DateTime.Now.AddDays(1)
            };

            user.PasswordHash = hasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Account created. Please confirm your email",
                User = user
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }


    public async Task<AuthResponse> RegisterMarshalAsync(string email, string password, string firstName,
        string lastName, string vehicleType, string licenseId)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };

            var hasher = new PasswordHasher<User>();
            var prefix = (vehicleType.Length >= 3 ? vehicleType[..3] : vehicleType).ToUpperInvariant();
            var marshal = new User
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.Now,
                Role = "Marshal",
                VehicleType = vehicleType,
                LicenseId = licenseId,
                VehicleId = $"VEH-{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                // Marshals are activated immediately; no email confirmation or wallet required.
                IsActive = true,
                IsEmailConfirmed = true
            };

            marshal.PasswordHash = hasher.HashPassword(marshal, password);

            _context.Users.Add(marshal);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Marshal account created",
                User = marshal
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
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

        if (!user.IsEmailConfirmed)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Please confirm your Email"
            };
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result != PasswordVerificationResult.Success)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid credentials"
            };
        }
        
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
        httpContext.Session.SetInt32("userId", user.Id);
        httpContext.Session.SetString("Username", user.FirstName);
    }

    public void ClearUserSession(HttpContext httpContext)
    {
        httpContext.Session.Clear();
    }

    public User? GetCurrentUser(HttpContext httpContext)
    {
        var userId = httpContext.Session.GetInt32("userId");
        if (userId == null)
            return null;

        return _context.Users.Find(userId);
    }

    public async Task<bool> ConfirmationEmailAsync(int userId, string token)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
            if (user.EmailConfirmationToken != token)
                return false;
            if (user.ConfirmationTokenExpiry < DateTime.Now)
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<AuthResponse> ForgotPasswordAsync(string email)
    {
        var check = await EmailExistsAsync(email);
        if (!check)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Email not found"
            };
        }
        
        await _emailService.SendEmailAsync(email, "Password Reset", "");
        return new AuthResponse
        {
            Success = true,
            Message = "Password Reset"
        };

    }
    
}