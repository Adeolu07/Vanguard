using Tripfinity.Interfaces;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tripfinity.Models.Data;

/// <summary>
/// Optional bootstrap of the administrator account. Credentials are never
/// hardcoded: seeding only runs when <c>Seed:Enabled</c> is true and both
/// <c>Seed:AdminEmail</c> and <c>Seed:AdminPassword</c> are supplied through
/// configuration (user secrets, environment variables or a key vault).
/// </summary>
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext context, IWalletService walletService, IConfiguration configuration,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _walletService = walletService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var adminEmail = _configuration["Seed:AdminEmail"];
        var adminPassword = _configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            _logger.LogWarning("Seeding skipped: Seed:AdminEmail and Seed:AdminPassword must both be configured");
            return;
        }

        var admin = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        var hasher = new PasswordHasher<User>();

        if (admin == null)
        {
            admin = new User
            {
                FirstName = "Admin",
                LastName = "Tripfinity",
                Email = adminEmail,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsEmailConfirmed = true,
                IsActive = true
            };

            admin.PasswordHash = hasher.HashPassword(admin, adminPassword);

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded administrator {UserId}", admin.Id);
        }

        if (string.IsNullOrEmpty(admin.UserWalletId))
        {
            var createReq = new CreateWalletRequest
            {
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                CustomerAlias = admin.Email,
            };

            var wallet = await _walletService.CreateWalletAsync(createReq);
            if (wallet?.ResponseHeader?.ResponseCode == "00" &&
                !string.IsNullOrEmpty(wallet.AccountDetails?.CustomerId))
            {
                admin.UserWalletId = wallet.AccountDetails.CustomerId;
                _context.Users.Update(admin);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Seeded administrator has no wallet: {Message}",
                    wallet?.ResponseHeader?.ResponseMessage);
            }
        }
    }
}