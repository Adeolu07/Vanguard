using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;
    private readonly ILogger<WalletService> _logger;

    public WalletService(AppDbContext context, ILogger<WalletService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<AuthenticationResponse> AuthenticationAsync(AuthenticationRequest request)
    {
        return Task.FromResult(new AuthenticationResponse
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Success" },
            Token = "internal",
            ExpiryDate = DateTime.Now.AddDays(1).ToString()
        });
    }

    public async Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest request)
    {
        _logger.LogInformation("Creating wallet for {FirstName} {LastName}", request.FirstName, request.LastName);
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == request.FirstName && u.LastName == request.LastName);

            if (user == null)
                return new CreateWalletResponse
                {
                    ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "User not found" },
                    AccountDetails = new AccountDetails
                    {
                        AccountNumber = "",
                        CustomerId = "",
                        CustomerAlias = "",
                        BankName = "",
                        BankCode = "",
                        FirstName = "",
                        LastName = "",
                        Bvn = ""
                    }
                };

            var existing = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);
            if (existing != null)
                return new CreateWalletResponse
                {
                    ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Wallet already exists" },
                    AccountDetails = new AccountDetails
                    {
                        AccountNumber = existing.CustomerId,
                        CustomerId = existing.CustomerId,
                        CustomerAlias = $"{user.FirstName} {user.LastName}",
                        BankName = "Tripfinity Wallet",
                        BankCode = "TRP",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Bvn = ""
                    }
                };

            var wallet = new Wallet
            {
                CustomerId = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Balance = 0,
                IsActive = true
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return new CreateWalletResponse
            {
                ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Wallet created" },
                AccountDetails = new AccountDetails
                {
                    AccountNumber = wallet.CustomerId,
                    CustomerId = wallet.CustomerId,
                    CustomerAlias = $"{user.FirstName} {user.LastName}",
                    BankName = "Tripfinity Wallet",
                    BankCode = "TRP",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Bvn = ""
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<CreditWalletResponse> CreditWalletAsync(CreditWalletRequest request)
    {
        _logger.LogInformation("Crediting wallet for {CustomerId}", request.CustomerId);
        try
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.CustomerId == request.CustomerId);
            if (wallet == null)
                return new CreditWalletResponse
                {
                    ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "Wallet not found" },
                    Amount = 0,
                    Balance = 0,
                    Description = "",
                    TransactionId = "",
                    TraceId = ""
                };

            wallet.Balance += request.Amount;

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                BalanceAfter = wallet.Balance,
                Type = "Credit",
                Description = request.Description,
                TraceId = request.TraceId,
                CreatedAt = DateTime.Now
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new CreditWalletResponse
            {
                ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Credit successful" },
                Amount = request.Amount,
                Balance = wallet.Balance,
                Description = request.Description,
                TransactionId = transaction.TransactionId,
                TraceId = request.TraceId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<DebitWalletResponse> DebitWalletAsync(DebitWalletRequest request)
    {
        _logger.LogInformation("Debiting wallet for {CustomerId}", request.CustomerId);
        try
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.CustomerId == request.CustomerId);
            if (wallet == null)
                return new DebitWalletResponse
                {
                    ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "Wallet not found" },
                    Amount = 0,
                    Balance = 0,
                    Description = "",
                    TransactionId = "",
                    TraceId = ""
                };

            if (wallet.Balance < request.Amount)
                return new DebitWalletResponse
                {
                    ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "Insufficient balance" },
                    Amount = 0,
                    Balance = wallet.Balance,
                    Description = "",
                    TransactionId = "",
                    TraceId = ""
                };

            wallet.Balance -= request.Amount;

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                BalanceAfter = wallet.Balance,
                Type = "Debit",
                Description = request.Description,
                TraceId = request.TraceId,
                CreatedAt = DateTime.Now
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new DebitWalletResponse
            {
                ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Debit successful" },
                Amount = request.Amount,
                Balance = wallet.Balance,
                Description = request.Description,
                TransactionId = transaction.TransactionId,
                TraceId = request.TraceId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request)
    {
        var wallet = await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerId == request.CustomerId);

        if (wallet == null)
            return new GetBalanceResponse
            {
                ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "Wallet not found" },
                Balance = 0
            };

        return new GetBalanceResponse
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Success" },
            Balance = wallet.Balance
        };
    }

    public async Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest request)
    {
        var transaction = await _context.WalletTransactions
            .FirstOrDefaultAsync(t => t.TransactionId == request.TransactionId);

        if (transaction == null)
            return new GetTransactionResponse
            {
                ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "Transaction not found" },
                TransactionDetails = null
            };

        return new GetTransactionResponse
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Success" },
            TransactionDetails = new TransactionDetails
            {
                TranType = transaction.Type,
                Amount = transaction.Amount,
                Description = transaction.Description,
                TransactionId = transaction.TransactionId,
                SessionId = transaction.TraceId,
                BankCode = "TRP",
                BankName = "Tripfinity Wallet"
            }
        };
    }

    public async Task<List<WalletTransaction>> GetTransactionHistoryAsync(string customerId)
    {
        return await _context.WalletTransactions
            .AsNoTracking()
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<RefundResponse> RefundAsync(RefundRequest request)
    {
        _logger.LogInformation("Processing refund for {TransactionId}", request.TransactionId);
        try
        {
            var original = await _context.WalletTransactions
                .FirstOrDefaultAsync(t => t.TransactionId == request.TransactionId);

            if (original == null)
                return new RefundResponse
                {
                    ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "Transaction not found" }
                };

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.CustomerId == original.CustomerId);

            if (wallet == null)
                return new RefundResponse
                {
                    ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = "Wallet not found" }
                };

            wallet.Balance += original.Amount;

            var refundTransaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                CustomerId = original.CustomerId,
                Amount = original.Amount,
                BalanceAfter = wallet.Balance,
                Type = "Refund",
                Description = $"Refund for {original.TransactionId}",
                TraceId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now
            };

            _context.WalletTransactions.Add(refundTransaction);
            await _context.SaveChangesAsync();

            return new RefundResponse
            {
                ResponseHeader = new ResponseHeader { ResponseCode = "00", ResponseMessage = "Refund successful" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task SaveUserWalletIdAsync(int userId, string walletId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.UserWalletId = walletId;
            _context.Users.Update(user);
        }

        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            _context.Wallets.Add(new Wallet
            {
                UserId = userId,
                CustomerId = walletId,
                Balance = 0,
                IsActive = true
            });
        }
        else
        {
            wallet.CustomerId = walletId;
        }

        await _context.SaveChangesAsync();
    }
}