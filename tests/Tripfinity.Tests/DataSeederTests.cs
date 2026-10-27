using Tripfinity.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tripfinity.Tests;

public class DataSeederTests
{
    private static IConfiguration Config(string? email, string? password) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:AdminEmail"] = email,
                ["Seed:AdminPassword"] = password
            })
            .Build();

    private static DataSeeder CreateSeeder(TestDatabase db, IConfiguration config, string? walletId = "admin-wallet") =>
        new(db.Context, new StubWalletService(walletId), config, NullLogger<DataSeeder>.Instance);

    [Fact]
    public async Task Nothing_is_created_when_credentials_are_not_configured()
    {
        using var db = new TestDatabase();

        await CreateSeeder(db, Config(null, null)).SeedAsync();

        Assert.Empty(db.Context.Users);
    }

    [Fact]
    public async Task Admin_is_created_confirmed_active_and_owns_a_wallet()
    {
        using var db = new TestDatabase();
        var config = Config("admin@example.com", "AdminPassword1");

        await CreateSeeder(db, config).SeedAsync();

        var admin = await db.Context.Users.SingleAsync();
        Assert.Equal("Admin", admin.Role);
        Assert.True(admin.IsEmailConfirmed);
        Assert.True(admin.IsActive);
        Assert.Equal("admin-wallet", admin.UserWalletId);

        var verified = new PasswordHasher<Tripfinity.Models.Tables.User>()
            .VerifyHashedPassword(admin, admin.PasswordHash, "AdminPassword1");
        Assert.Equal(PasswordVerificationResult.Success, verified);
    }

    [Fact]
    public async Task Admin_can_still_sign_in_when_wallet_creation_fails()
    {
        using var db = new TestDatabase();
        var config = Config("admin@example.com", "AdminPassword1");

        await CreateSeeder(db, config, walletId: null).SeedAsync();

        var admin = await db.Context.Users.SingleAsync();
        Assert.True(admin.IsEmailConfirmed);
        Assert.True(admin.IsActive);
        Assert.Null(admin.UserWalletId);
    }

    [Fact]
    public async Task Seeding_is_idempotent()
    {
        using var db = new TestDatabase();
        var config = Config("admin@example.com", "AdminPassword1");

        await CreateSeeder(db, config).SeedAsync();
        await CreateSeeder(db, config).SeedAsync();

        Assert.Equal(1, await db.Context.Users.CountAsync());
    }
}
