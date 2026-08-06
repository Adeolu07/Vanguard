using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, IWalletService walletService, IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _walletService = walletService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponse> SignUpAsync(RegisterViewModel model)
    {
        _logger.LogInformation("Passenger Signup for {Email}", model.Email);

        var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
        if (existingUser)
        {
            _logger.LogWarning("Email already exists");
            return new AuthResponse { Success = false, Message = "Email already exists" };
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            CreatedAt = DateTime.Now,
            IsActive = false,
            Role = "Passenger",
            EmailConfirmationToken = Guid.NewGuid().ToString(),
            ConfirmationTokenExpiry = DateTime.Now.AddDays(1)
        };
        user.PasswordHash = hasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Passenger account created: {Email}", user.Email);
        return new AuthResponse { Success = true, Message = "Account created. Please confirm your email", User = user };
    }

    public async Task<AuthResponse> RegisterMarshalAsync(MarshalRegisterViewModel model)
    {
        _logger.LogInformation("Registering new marshal");

        var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
        if (existingUser)
        {
            _logger.LogWarning("Marshal registration failed: email {Email} already exists", model.Email);
            return new AuthResponse { Success = false, Message = "Email already exists" };
        }

        var hasher = new PasswordHasher<User>();
        var prefix = (model.VehicleType.Length >= 3 ? model.VehicleType[..3] : model.VehicleType).ToUpper();

        var marshal = new User
        {
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.Now,
            PhoneNumber = model.PhoneNumber,
            Role = "Marshal",
            VehicleType = model.VehicleType,
            LicenseId = model.LicenseId,
            VehicleId = $"VEH-{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            IsActive = true,
            IsEmailConfirmed = true
        };

        marshal.PasswordHash = hasher.HashPassword(marshal, model.Password);
        _context.Users.Add(marshal);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Marshal Account created: {Email}", marshal.Email);
        return new AuthResponse { Success = true, Message = "Marshal account created", User = marshal };
    }

    public async Task<AuthResponse?> SignInAsync(string email, string password)
    {

        _logger.LogInformation("User sign in");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            _logger.LogWarning("Sign in failed: user {Email} not found", email);
            return new AuthResponse { Success = false, Message = "User not found" };
        }

        if (!user.IsEmailConfirmed)
        {
            _logger.LogWarning("Sign-in failed: email {Email} not confirmed", email);
            return new AuthResponse { Success = false, Message = "Please confirm your Email" };
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result != PasswordVerificationResult.Success)
        {
            _logger.LogInformation("Sign-in failed: invalid credentials for: {Email}", email);
            return new AuthResponse { Success = false, Message = "Invalid credentials" };
        }

        _logger.LogInformation("User {Email} signed in successfully", email);
        return new AuthResponse { Success = true, Message = "Sign in successful", User = user };
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task SetUserSession(HttpContext httpContext, User user)
    {
        httpContext.Session.SetInt32("userId", user.Id);
        httpContext.Session.SetString("Username", user.FirstName);
        httpContext.Session.SetString("Role", user.Role!);
        if (user.Role == "Marshal")
        {
            httpContext.Session.SetInt32("marshalId", user.Id);
            httpContext.Session.SetString("marshalVehicleType", user.VehicleType ?? "");
            httpContext.Session.SetString("marshalVehicleId", user.VehicleId ?? "");
        }
        await httpContext.Session.CommitAsync();
    }

    public void ClearUserSession(HttpContext httpContext)
    {
        httpContext.Session.Clear();
    }

    public User? GetCurrentUser(HttpContext httpContext)
    {
        var userId = httpContext.Session.GetInt32("userId");
        if (userId == null) return null;
        return _context.Users.Find(userId);
    }

    public async Task<bool> ConfirmationEmailAsync(int userId, string token)
    {
        _logger.LogInformation("Email confirmation for user {UserId}", userId);

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Email confirmation for user {UserId} failed, not found", userId);
            return false;
        }

        if (user.IsEmailConfirmed)
        {
            _logger.LogInformation("Email confirmation for user {UserId} is already confirmed", userId);
            return true;
        }

        if (user.EmailConfirmationToken != token)
        {
            _logger.LogInformation("Confirmation failed: token mismatch for user {UserId}", userId);
            return false;
        }

        if (user.ConfirmationTokenExpiry < DateTime.Now)
        {
            _logger.LogWarning("Confirmation failed: token expired for user {UserId}", userId);
            return false;
        }

        user.IsEmailConfirmed = true;
        user.IsActive = true;
        user.EmailConfirmationToken = null;
        user.ConfirmationTokenExpiry = null;

        if (string.IsNullOrEmpty(user.UserWalletId))
        {
            _logger.LogInformation("Wallet Creation");
            var createWalletRequest = new CreateWalletRequest
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CustomerAlias = user.Email,
            };

            var createWalletResponse = await _walletService.CreateWalletAsync(createWalletRequest);
            if (createWalletResponse?.ResponseHeader?.ResponseCode == "00")
            {
                user.UserWalletId = createWalletResponse.AccountDetails.CustomerId;
                _logger.LogInformation("Wallet Creation Successful for user with id {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Wallet creation returned non-success for user {UserId}: {Code} - {Message}",
                    userId, createWalletResponse?.ResponseHeader?.ResponseCode,
                    createWalletResponse?.ResponseHeader?.ResponseMessage);
            }
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email confirmation for user {UserId} completed", userId);
        return true;
    }

    public async Task<AuthResponse> ForgotPasswordAsync(string email)
    {
        _logger.LogInformation("Forgot password for {Email}", email);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return new AuthResponse { Success = true, Message = "If your email exists, a reset link has been sent." };
        }

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpiry = DateTime.Now.AddHours(1);
        await _context.SaveChangesAsync();

        var resetLink = $"https://localhost:5001/Auth/ResetPassword?email={Uri.EscapeDataString(email)}&token={user.PasswordResetToken}";
        await _emailService.SendEmailAsync(email, "Reset Your Password",
            $"Click the link below to reset your password:\n{resetLink}\n\nThis link expires in 1 hour.");

        _logger.LogInformation("Password reset email sent to {Email}", email);
        return new AuthResponse { Success = true, Message = "If your email exists, a reset link has been sent." };
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string email, string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        return user.PasswordResetToken == token &&
               user.PasswordResetTokenExpiry.HasValue &&
               user.PasswordResetTokenExpiry > DateTime.Now;
    }

    public async Task<AuthResponse> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var valid = await ValidatePasswordResetTokenAsync(email, token);
        if (!valid)
            return new AuthResponse { Success = false, Message = "Invalid or expired reset token." };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return new AuthResponse { Success = false, Message = "User not found." };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, newPassword);

        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Password reset successful for {Email}", email);
        return new AuthResponse { Success = true, Message = "Password has been reset successfully." };
    }
}