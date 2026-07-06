using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
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

    public AuthService(AppDbContext context,  IWalletService walletService,  IEmailService emailService,
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
        try
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (existingUser)
            {
                _logger.LogWarning("Email already exists");
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }
            
            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber =  model.PhoneNumber,
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
            return new AuthResponse
            {
                Success = true,
                Message = "Account created. Please confirm your email",
                User = user
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during passenger sign-up");
            return  new AuthResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<AuthResponse> RegisterMarshalAsync(MarshalRegisterViewModel model)
    {
        _logger.LogInformation("Registering new marshal");
        try
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (existingUser)
            {
                _logger.LogWarning("Marshal registration failed: email {Email} already exists", model.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }
            
            var hasher = new PasswordHasher<User>();
            var prefix = (model.VehicleType.Length >= 3 
                ? model.VehicleType[..3] :
                model.VehicleType).ToUpper();
            
            var marshal = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.Now,
                PhoneNumber =  model.PhoneNumber,
                Role = "Marshal",
                VehicleType = model.VehicleType,
                LicenseId = model.LicenseId,
                VehicleId = $"VEH-{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                // Marshals are activated immediately; no email confirmation or wallet required. for now sha
                IsActive = true,
                IsEmailConfirmed = true
            };

            marshal.PasswordHash = hasher.HashPassword(marshal, model.Password);

            _context.Users.Add(marshal);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Marshal Account created: {Email}", marshal.Email);
            return new AuthResponse
            {
                Success = true,
                Message = "Marshal account created",
                User = marshal
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during marshal registration");
            return new AuthResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
    
    public async Task<AuthResponse?> SignInAsync(string email,  string password)
    {
        _logger.LogInformation("User sign in");
        
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Sign in failed: user {Email} not found",  email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            if (!user.IsEmailConfirmed)
            {
                _logger.LogWarning("Sign-in failed: email {Email} not confirmed", email);
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
                _logger.LogInformation("Sign -in failed: invalid credentials for: {Email}", email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            _logger.LogInformation("User {Email} signed in successfully", email);
            return new AuthResponse
            {
                Success = true,
                Message = "Sign in successful",
                User = user
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-in for {Email}", email);
            return new AuthResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during email exists check");
            return false;
        }
    }

    

    public async Task SetUserSession(HttpContext httpContext, User user)
    {
        httpContext.Session.SetInt32("userId", user.Id);
        httpContext.Session.SetString("Username", user.FirstName);
        await httpContext.Session.CommitAsync();
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
        try
        {
            return _context.Users.Find(userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during get current user from DB");
            return null;
        }
    }

    public async Task<bool> ConfirmationEmailAsync(int userId, string token)
    {
        _logger.LogInformation("Email confirmation for user {UserId}", userId);
        try
        {
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
                try
                {
                    _logger.LogInformation("Wallet Creation");
                    var createWalletRequest = new CreateWalletRequest
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                    };
                    
                    var createWalletResponse = await _walletService.CreateWalletAsync(createWalletRequest);

                    if (createWalletResponse?.ResponseHeader?.ResponseCode == "00" )
                    {
                        user.UserWalletId = createWalletResponse.AccountDetails.CustomerId;
                        _logger.LogInformation("Wallet Creation Successful for user with id {UserId}", userId);
                    }

                    else
                    {
                        _logger.LogWarning(
                            "Wallet creation returned non-success for user {UserId}: {Code} - {Message}",
                            userId,
                            createWalletResponse?.ResponseHeader?.ResponseCode,
                            createWalletResponse?.ResponseHeader?.ResponseMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Wallet creation threw an exception for user {UserId}", userId);
                }
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Email confirmation for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during email confirmation");
            return false;
        }
    }

    public async Task<AuthResponse> ForgotPasswordAsync(string email)
    {
        _logger.LogInformation("Forgot password for {Email}", email);
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Forgot password for non-existent email: {Email}", email);
                return new AuthResponse { Success = true, Message = "If your email exists, a reset link has been sent." };
            }
            
            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiry = DateTime.Now.AddHours(1); // valid for 1 hour
            await _context.SaveChangesAsync();

            // Build reset link
            var resetLink = $"https://localhost:5001/Auth/ResetPassword?email={Uri.EscapeDataString(email)}&token={user.PasswordResetToken}";


            var confirmationLink =
                $"http://localhost/auth/ResetPassword??userId={user.Id}&token={user.PasswordResetToken}?";
            // Send email
            await _emailService.SendEmailAsync(
                email,
                "Reset Your Password",
                $"Click the link below to reset your password:\n{resetLink}\n\nThis link expires in 1 hour.");

            _logger.LogInformation("Password reset email sent to {Email}", email);
            return new AuthResponse { Success = true, Message = "If your email exists, a reset link has been sent." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for {Email}", email);
            return new AuthResponse { Success = false, Message = "Something went wrong. Please try again." };
        }
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
        try
        {
            var valid = await ValidatePasswordResetTokenAsync(email, token);
            if (!valid)
                return new AuthResponse { Success = false, Message = "Invalid or expired reset token." };

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new AuthResponse { Success = false, Message = "User not found." };

            // Hash the new password
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, newPassword);

            // Clear the reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset successful for {Email}", email);
            return new AuthResponse { Success = true, Message = "Password has been reset successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for {Email}", email);
            return new AuthResponse { Success = false, Message = "An error occurred. Please try again." };
        }
    }
    
}