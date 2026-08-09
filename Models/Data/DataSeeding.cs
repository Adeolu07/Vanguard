using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Models.Data;

public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;

    public DataSeeder(AppDbContext context, IWalletService walletService)
    {
        _context = context;
        _walletService = walletService;
    }

    public async Task SeedAsync()
    {
        // ---- Admin user ----
        const string adminEmail = "admin@tripfinity.com";

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
                CreatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = hasher.HashPassword(admin, "Admin123");
            
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }

        // Ensure admin has a wallet
        if (string.IsNullOrEmpty(admin.UserWalletId))
        {
            var createReq = new CreateWalletRequest
            {
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                CustomerAlias = "morakinyo.aj@proton.me",
            };

            var wallet = await _walletService.CreateWalletAsync(createReq);
            if (wallet?.ResponseHeader?.ResponseCode == "00" &&
                !string.IsNullOrEmpty(wallet.AccountDetails?.CustomerId))
            {
                admin.UserWalletId = wallet.AccountDetails.CustomerId;
                _context.Users.Update(admin);
                await _context.SaveChangesAsync();
            }
        }
    }
}