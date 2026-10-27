using Tripfinity.Interfaces;
using Tripfinity.Models.Data;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Tables;
using Tripfinity.Models.ViewModels;
using Tripfinity.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Tripfinity.Tests;

/// <summary>
/// SQLite in-memory database. It supports the raw SQL used for atomic seat
/// reservation, which the EF in-memory provider does not.
/// </summary>
internal sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public async Task<User> AddUserAsync(string email, string password, string role = "Passenger",
        bool confirmed = true, bool active = true, string? walletId = "wallet-1")
    {
        var user = new User
        {
            Email = email,
            FirstName = "Test",
            LastName = "User",
            Role = role,
            IsEmailConfirmed = confirmed,
            IsActive = active,
            UserWalletId = walletId,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
            .HashPassword(user, password);

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}

internal sealed class TestDataProtection
{
    public static TicketQrPayload CreatePayload() =>
        new(DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "tripfinity-tests"))));

    public static IConfiguration Configuration(bool acceptLegacyUnsignedQr = false) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tickets:AcceptLegacyUnsignedQr"] = acceptLegacyUnsignedQr.ToString()
            })
            .Build();
}

/// <summary>Returns a canned wallet response so the seeding path can be tested offline.</summary>
internal sealed class StubWalletService : IWalletService
{
    private readonly string? _customerId;

    public StubWalletService(string? customerId) => _customerId = customerId;

    public Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest createWallet)
    {
        if (_customerId is null)
        {
            return Task.FromResult(new CreateWalletResponse
            {
                ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "wallet service down" },
                AccountDetails = null!
            });
        }

        return Task.FromResult(new CreateWalletResponse
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "ok" },
            AccountDetails = new AccountDetails
            {
                AccountNumber = "0123456789",
                CustomerId = _customerId,
                CustomerAlias = createWallet.CustomerAlias,
                BankName = "Test Bank",
                BankCode = "000014",
                FirstName = createWallet.FirstName,
                LastName = createWallet.LastName,
                Bvn = ""
            }
        });
    }

    public Task<WalletTransaction> CreditWalletAsync(CreditWalletRequest creditWallet) => throw new NotSupportedException();
    public Task<WalletTransaction> DebitWalletAsync(DebitWalletRequest debitWallet) => throw new NotSupportedException();
    public Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest getBalance) => throw new NotSupportedException();
    public Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest getTransaction) => throw new NotSupportedException();
    public Task<WalletNameEnquiryResponse> WalletNameEnquiryAsync(WalletNameEnquiryRequest request) => throw new NotSupportedException();
    public Task<RefundResponse> RefundAsync(RefundRequest refund) => throw new NotSupportedException();
    public Task<GetTransactionListResponse> GetTransactionList(GetTransactionListRequest request) => throw new NotSupportedException();
    public Task EnsureAuthenticatedAsync() => Task.CompletedTask;
    public Task<MarshalWalletViewModel> BuildWalletInfoAsync(string walletId, int page) => throw new NotSupportedException();
}

internal sealed class FakeEmailService : IEmailService
{
    public List<(string Email, string Link)> ConfirmationEmails { get; } = new();
    public List<(string Email, string Link)> ResetEmails { get; } = new();

    public Task SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        ConfirmationEmails.Add((email, confirmationLink));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        ResetEmails.Add((email, resetLink));
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage) => Task.CompletedTask;
}

internal sealed class FakeCipService : ICipService
{
    public Task<NameEnquiryWrapper> AccountEnquiry(string accountNumber, string bankCode) =>
        Task.FromResult(new NameEnquiryWrapper { Status = false, Message = "Not used in tests" });
}

internal sealed class UnusedWalletService : IWalletService
{
    public Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest createWallet) => throw new NotSupportedException();
    public Task<WalletTransaction> CreditWalletAsync(CreditWalletRequest creditWallet) => throw new NotSupportedException();
    public Task<WalletTransaction> DebitWalletAsync(DebitWalletRequest debitWallet) => throw new NotSupportedException();
    public Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest getBalance) => throw new NotSupportedException();
    public Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest getTransaction) => throw new NotSupportedException();
    public Task<WalletNameEnquiryResponse> WalletNameEnquiryAsync(WalletNameEnquiryRequest request) => throw new NotSupportedException();
    public Task<RefundResponse> RefundAsync(RefundRequest refund) => throw new NotSupportedException();
    public Task<GetTransactionListResponse> GetTransactionList(GetTransactionListRequest request) => throw new NotSupportedException();
    public Task EnsureAuthenticatedAsync() => throw new NotSupportedException();
    public Task<MarshalWalletViewModel> BuildWalletInfoAsync(string walletId, int page) => throw new NotSupportedException();
}

internal sealed class FakePaymentService : IPaymentService
{
    private readonly bool _succeed;

    public FakePaymentService(bool succeed) => _succeed = succeed;

    public int PaymentAttempts { get; private set; }

    public Task<PaymentResult> ProcessPaymentAsync(User user, Booking booking)
    {
        PaymentAttempts++;
        return Task.FromResult(new PaymentResult
        {
            Success = _succeed,
            ErrorMessage = _succeed ? null : "Payment failed"
        });
    }

    public Task<PaymentResult> ProcessCancellationAsync(Booking booking, User user, bool isMarshal, DateTime tripTime) =>
        Task.FromResult(new PaymentResult { Success = true, Refunded = true });
}
