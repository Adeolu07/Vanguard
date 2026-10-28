using Tripfinity.Interfaces;
using Tripfinity.Models.Data;
using Tripfinity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tripfinity.Tests;

public class AuthServiceTests
{
    private static AuthService CreateService(AppDbContext context, FakeEmailService? email = null,
        IWalletService? wallet = null) =>
        new(context, wallet ?? new UnusedWalletService(), email ?? new FakeEmailService(), new FakeCipService(),
            NullLogger<AuthService>.Instance);

    private static async Task<Models.Tables.User> AddUnconfirmedUserWithTokenAsync(
        TestDatabase db, string email, string token, string? walletId = null)
    {
        var user = await db.AddUserAsync(email, "CorrectHorse1", confirmed: false, active: false, walletId: walletId);
        user.EmailConfirmationToken = token;
        user.ConfirmationTokenExpiry = DateTime.UtcNow.AddHours(1);
        await db.Context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Confirmation_still_succeeds_when_the_wallet_service_is_down()
    {
        using var db = new TestDatabase();
        var user = await AddUnconfirmedUserWithTokenAsync(db, "new@example.com", "token-1");
        var service = CreateService(db.Context, wallet: new StubWalletService(customerId: null));

        var confirmed = await service.ConfirmationEmailAsync(user.Id, "token-1");

        Assert.True(confirmed);
        var reloaded = await db.Context.Users.FindAsync(user.Id);
        Assert.True(reloaded!.IsEmailConfirmed);
        Assert.True(reloaded.IsActive);
        Assert.Null(reloaded.UserWalletId);
        Assert.Null(reloaded.EmailConfirmationToken);
    }

    [Fact]
    public async Task Confirmation_assigns_the_wallet_when_creation_succeeds()
    {
        using var db = new TestDatabase();
        var user = await AddUnconfirmedUserWithTokenAsync(db, "new@example.com", "token-2");
        var service = CreateService(db.Context, wallet: new StubWalletService("cust-123"));

        var confirmed = await service.ConfirmationEmailAsync(user.Id, "token-2");

        Assert.True(confirmed);
        var reloaded = await db.Context.Users.FindAsync(user.Id);
        Assert.Equal("cust-123", reloaded!.UserWalletId);
    }

    [Fact]
    public async Task Confirmation_with_a_wrong_token_does_not_confirm()
    {
        using var db = new TestDatabase();
        var user = await AddUnconfirmedUserWithTokenAsync(db, "new@example.com", "right-token");
        var service = CreateService(db.Context, wallet: new StubWalletService("cust-123"));

        var confirmed = await service.ConfirmationEmailAsync(user.Id, "wrong-token");

        Assert.False(confirmed);
        var reloaded = await db.Context.Users.FindAsync(user.Id);
        Assert.False(reloaded!.IsEmailConfirmed);
    }

    [Fact]
    public async Task Deferred_wallet_can_be_created_later_and_sign_in_works_meanwhile()
    {
        using var db = new TestDatabase();
        var user = await AddUnconfirmedUserWithTokenAsync(db, "late@example.com", "token-3");
        var wallet = new StubWalletService(customerId: null);

        var auth = CreateService(db.Context, wallet: wallet);
        await auth.ConfirmationEmailAsync(user.Id, "token-3");

        // Sign-in is possible even though the wallet is missing.
        var signIn = await auth.SignInAsync("late@example.com", "CorrectHorse1");
        Assert.True(signIn!.Success);

        var passenger = new PassengerService(db.Context, new StubWalletService("cust-late"),
            NullLogger<PassengerService>.Instance,
            new TicketService(db.Context, NullLogger<TicketService>.Instance,
                TestDataProtection.CreatePayload(), TestDataProtection.Configuration()));

        var retry = await passenger.CreateWalletIfMissingAsync(user.Id);

        Assert.True(retry.Success);
        Assert.Equal("cust-late", (await db.Context.Users.FindAsync(user.Id))!.UserWalletId);
    }

    [Fact]
    public async Task Wallet_setup_reports_failure_without_breaking_the_account()
    {
        using var db = new TestDatabase();
        var user = await AddUnconfirmedUserWithTokenAsync(db, "still@example.com", "token-4");
        var auth = CreateService(db.Context, wallet: new StubWalletService(customerId: null));
        await auth.ConfirmationEmailAsync(user.Id, "token-4");

        var passenger = new PassengerService(db.Context, new StubWalletService(customerId: null),
            NullLogger<PassengerService>.Instance,
            new TicketService(db.Context, NullLogger<TicketService>.Instance,
                TestDataProtection.CreatePayload(), TestDataProtection.Configuration()));

        var retry = await passenger.CreateWalletIfMissingAsync(user.Id);

        Assert.False(retry.Success);
        var reloaded = await db.Context.Users.FindAsync(user.Id);
        Assert.True(reloaded!.IsEmailConfirmed);
        Assert.Null(reloaded.UserWalletId);
    }

    [Fact]
    public async Task Wallet_setup_is_a_no_op_when_a_wallet_already_exists()
    {
        using var db = new TestDatabase();
        var user = await db.AddUserAsync("has@example.com", "CorrectHorse1", walletId: "existing-wallet");

        var passenger = new PassengerService(db.Context, new StubWalletService("should-not-be-used"),
            NullLogger<PassengerService>.Instance,
            new TicketService(db.Context, NullLogger<TicketService>.Instance,
                TestDataProtection.CreatePayload(), TestDataProtection.Configuration()));

        var result = await passenger.CreateWalletIfMissingAsync(user.Id);

        Assert.True(result.Success);
        Assert.Equal("existing-wallet", (await db.Context.Users.FindAsync(user.Id))!.UserWalletId);
    }

    [Fact]
    public async Task Unknown_email_and_wrong_password_return_the_same_message()
    {
        using var db = new TestDatabase();
        await db.AddUserAsync("known@example.com", "CorrectHorse1");
        var service = CreateService(db.Context);

        var unknown = await service.SignInAsync("nobody@example.com", "CorrectHorse1");
        var wrongPassword = await service.SignInAsync("known@example.com", "WrongPassword1");

        Assert.NotNull(unknown);
        Assert.NotNull(wrongPassword);
        Assert.False(unknown!.Success);
        Assert.False(wrongPassword!.Success);
        Assert.Equal(unknown.Message, wrongPassword.Message);
        Assert.DoesNotContain("not found", unknown.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unconfirmed_account_cannot_sign_in()
    {
        using var db = new TestDatabase();
        await db.AddUserAsync("pending@example.com", "CorrectHorse1", confirmed: false);
        var service = CreateService(db.Context);

        var result = await service.SignInAsync("pending@example.com", "CorrectHorse1");

        Assert.NotNull(result);
        Assert.False(result!.Success);
        Assert.Contains("confirm", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unconfirmed_account_still_needs_the_correct_password()
    {
        using var db = new TestDatabase();
        await db.AddUserAsync("pending@example.com", "CorrectHorse1", confirmed: false);
        var service = CreateService(db.Context);

        var result = await service.SignInAsync("pending@example.com", "WrongPassword1");

        Assert.NotNull(result);
        Assert.False(result!.Success);
        Assert.DoesNotContain("confirm", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Inactive_account_cannot_sign_in()
    {
        using var db = new TestDatabase();
        await db.AddUserAsync("disabled@example.com", "CorrectHorse1", active: false);
        var service = CreateService(db.Context);

        var result = await service.SignInAsync("disabled@example.com", "CorrectHorse1");

        Assert.NotNull(result);
        Assert.False(result!.Success);
        Assert.Equal("Invalid email or password.", result.Message);
    }

    [Fact]
    public async Task Valid_credentials_return_the_user()
    {
        using var db = new TestDatabase();
        var user = await db.AddUserAsync("ok@example.com", "CorrectHorse1");
        var service = CreateService(db.Context);

        var result = await service.SignInAsync("ok@example.com", "CorrectHorse1");

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal(user.Id, result.User!.Id);
    }

    [Fact]
    public async Task Forgot_password_does_not_reveal_whether_the_account_exists()
    {
        using var db = new TestDatabase();
        await db.AddUserAsync("known@example.com", "CorrectHorse1");
        var email = new FakeEmailService();
        var service = CreateService(db.Context, email);

        var known = await service.ForgotPasswordAsync("known@example.com", "https://host/reset?token=__TOKEN__");
        var unknown = await service.ForgotPasswordAsync("nobody@example.com", "https://host/reset?token=__TOKEN__");

        Assert.Equal(known.Message, unknown.Message);
        Assert.True(known.Success);
        Assert.True(unknown.Success);

        // Only the real account receives a message.
        Assert.Single(email.ResetEmails);
        Assert.DoesNotContain("__TOKEN__", email.ResetEmails[0].Link);
    }

    [Fact]
    public async Task Reset_password_rejects_an_invalid_token()
    {
        using var db = new TestDatabase();
        await db.AddUserAsync("known@example.com", "CorrectHorse1");
        var service = CreateService(db.Context);

        await service.ForgotPasswordAsync("known@example.com", "https://host/reset?token=__TOKEN__");
        var result = await service.ResetPasswordAsync("known@example.com", "not-the-token", "NewPassword1");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Reset_password_with_a_valid_token_replaces_the_credential()
    {
        using var db = new TestDatabase();
        await db.AddUserAsync("known@example.com", "CorrectHorse1");
        var email = new FakeEmailService();
        var service = CreateService(db.Context, email);

        await service.ForgotPasswordAsync("known@example.com", "https://host/reset?token=__TOKEN__");
        var token = new Uri(email.ResetEmails[0].Link).Query.TrimStart('?').Split('=')[^1];

        var reset = await service.ResetPasswordAsync("known@example.com", token, "NewPassword1");
        var oldPassword = await service.SignInAsync("known@example.com", "CorrectHorse1");
        var newPassword = await service.SignInAsync("known@example.com", "NewPassword1");

        Assert.True(reset.Success);
        Assert.False(oldPassword!.Success);
        Assert.True(newPassword!.Success);
    }
}
