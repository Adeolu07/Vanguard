using Tripfinity.Interfaces;
using Tripfinity.Models.Data;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Tables;
using Tripfinity.Models.ViewModels;
using Tripfinity.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Tripfinity.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly IEmailService _emailService;
    private readonly ICipService _cipService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, IWalletService walletService, IEmailService emailService, ICipService cipService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _walletService = walletService;
        _emailService = emailService;
        _cipService = cipService;
        _logger = logger;
    }

    public async Task<AuthResponse> SignUpAsync(RegisterViewModel model)
    {
        var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
        if (existingUser)
        {
            // Return the same shape as a successful signup: revealing that an
            // address is registered turns this form into an enumeration oracle.
            _logger.LogWarning("Passenger signup rejected: email already registered");
            return new AuthResponse
            {
                Success = true,
                Message = "Account created. Please confirm your email",
                User = null
            };
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            Role = "Passenger",
            EmailConfirmationToken = Guid.NewGuid().ToString(),
            ConfirmationTokenExpiry = DateTime.UtcNow.AddDays(1)
        };
        user.PasswordHash = hasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Passenger account created: {UserId}", user.Id);
        return new AuthResponse { Success = true, Message = "Account created. Please confirm your email", User = user };
    }

            public async Task<AuthResponse> RegisterMarshalAsync(MarshalRegisterViewModel model)
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (existingUser)
            {
                _logger.LogWarning("Marshal registration rejected: email already registered");
                return new AuthResponse
                {
                    Success = true,
                    Message = "Marshal account created",
                    User = null
                };
            }

            var accountNumber = model.AccountNumber?.Trim();
            var bankCode = model.BankCode;

            var hasAccount = !string.IsNullOrWhiteSpace(accountNumber);
            var hasBank = !string.IsNullOrWhiteSpace(bankCode);
            if (hasAccount != hasBank)
                return new AuthResponse { Success = false, Message = "Provide both account number and bank, or leave both blank." };

            string? verifiedAccountName = null;
            if (hasAccount && hasBank)
            {
                var enquiry = await _cipService.AccountEnquiry(accountNumber!, bankCode!);

                if (enquiry?.Data?.ResponseCode != "00" || string.IsNullOrWhiteSpace(enquiry?.Data?.AccountName))
                {
                    var reason = enquiry?.Message ?? enquiry?.Data?.ResponseMessage ?? "Name enquiry failed.";
                        _logger.LogWarning("CIP name enquiry failed during marshal registration: {Reason}", reason);
                    return new AuthResponse { Success = false, Message = $"Could not verify account: {reason}" };
                }

                if (!NameMatching.Matches(enquiry.Data.AccountName, model.FirstName, model.LastName))
                {
                    _logger.LogWarning("Account name mismatch while registering user");
                    return new AuthResponse { Success = false, Message = "Account name does not match your registered name." };
                }

                verifiedAccountName = enquiry.Data.AccountName;
            }

            var hasher = new PasswordHasher<User>();
            var prefix = (model.VehicleType.Length >= 3 ? model.VehicleType[..3] : model.VehicleType).ToUpper();

            var marshal = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                PhoneNumber = model.PhoneNumber,
                Role = "Marshal",
                VehicleType = model.VehicleType,
                LicenseId = model.LicenseId,
                VehicleId = $"VEH-{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                IsActive = false,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString(),
                ConfirmationTokenExpiry = DateTime.UtcNow.AddDays(1)
            };

            marshal.PasswordHash = hasher.HashPassword(marshal, model.Password);
            _context.Users.Add(marshal);
            await _context.SaveChangesAsync();

            if (verifiedAccountName is not null)
            {
                _context.MarshalBankAccounts.Add(new MarshalBankAccount
                {
                    MarshalId = marshal.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    AccountNumber = accountNumber!,
                    BankCode = bankCode!,
                    BankName = Banks.GetBankName(bankCode) ?? "",
                    AccountName = verifiedAccountName,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Marshal account created: {UserId}", marshal.Id);
            return new AuthResponse { Success = true, Message = "Marshal account created", User = marshal };
        }

    public async Task<AuthResponse?> SignInAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Always compare against a hash so a missing account and a wrong password
        // take the same path and return the same message.
        var hasher = new PasswordHasher<User>();
        var verification = user is null
            ? PasswordVerificationResult.Failed
            : hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (user is null || verification == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Sign-in failed: invalid credentials");
            return new AuthResponse { Success = false, Message = "Invalid email or password." };
        }

        if (!user.IsEmailConfirmed)
        {
            _logger.LogWarning("Sign-in refused: email not confirmed for user {UserId}", user.Id);
            return new AuthResponse { Success = false, Message = "Please confirm your email address before signing in." };
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Sign-in refused: inactive account {UserId}", user.Id);
            return new AuthResponse { Success = false, Message = "Invalid email or password." };
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("User {UserId} signed in", user.Id);
        return new AuthResponse { 
            Success = true, 
            Message = "Sign in successful", 
            User = user 
        };
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task SetUserSession(HttpContext httpContext, User user)
    {
        // Discard any pre-authentication session so a fixated session id cannot
        // be reused after sign-in.
        httpContext.Session.Clear();

        httpContext.Session.SetInt32("userId", user.Id);
        httpContext.Session.SetString("FirstName", user.FirstName);
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

        if (string.IsNullOrWhiteSpace(token))
            return false;

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Email confirmation failed: user {UserId} not found", userId);
            return false;
        }

        if (user.IsEmailConfirmed)
        {
            _logger.LogInformation("Email confirmation for user {UserId} is already confirmed", userId);
            return true;
        }

        if (user.EmailConfirmationToken is null || !TokenEquals(user.EmailConfirmationToken, token))
        {
            _logger.LogInformation("Confirmation failed: token mismatch for user {UserId}", userId);
            return false;
        }

        if (user.ConfirmationTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Confirmation failed: token expired for user {UserId}", userId);
            return false;
        }

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
            if (createWalletResponse?.ResponseHeader?.ResponseCode == "00"
                && !string.IsNullOrEmpty(createWalletResponse.AccountDetails?.CustomerId))
            {
                user.UserWalletId = createWalletResponse.AccountDetails.CustomerId;
                _logger.LogInformation("Wallet Creation Successful for user with id {UserId}", userId);
            }
            else
            {
                // A wallet-service outage must not cost the user their account:
                // confirm the email anyway and let them finish wallet setup from
                // the wallet page once the service recovers.
                _logger.LogWarning("Wallet creation deferred for user {UserId}: {Code} - {Message}",
                    userId, createWalletResponse?.ResponseHeader?.ResponseCode,
                    createWalletResponse?.ResponseHeader?.ResponseMessage);
            }
        }

        user.IsEmailConfirmed = true;
        user.IsActive = true;
        user.EmailConfirmationToken = null;
        user.ConfirmationTokenExpiry = null;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email confirmation for user {UserId} completed", userId);
        return true;
    }

    private static bool TokenEquals(string expected, string provided)
    {
        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expected);
        var providedBytes = System.Text.Encoding.UTF8.GetBytes(provided);
        return expectedBytes.Length == providedBytes.Length
               && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    public async Task<AuthResponse> ForgotPasswordAsync(string email, string resetLinkTemplate)
    {
        // The same message is returned whether or not the address exists.
        const string genericMessage = "If an account exists for that address, a reset link has been sent.";

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            _logger.LogWarning("Password reset requested for an unknown address");
            return new AuthResponse { Success = true, Message = genericMessage };
        }

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        var resetLink = resetLinkTemplate.Replace("__TOKEN__", user.PasswordResetToken);

        await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

        _logger.LogInformation("Password reset email sent to user {UserId}", user.Id);
        return new AuthResponse { Success = true, Message = genericMessage };
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string email, string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        return user.PasswordResetToken is not null
               && TokenEquals(user.PasswordResetToken, token)
               && user.PasswordResetTokenExpiry.HasValue
               && user.PasswordResetTokenExpiry > DateTime.UtcNow;
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