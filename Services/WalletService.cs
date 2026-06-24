using System.Net.Http.Headers;
using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class WalletService : IWalletService
{
    private readonly ILogger<WalletService> _logger;
    private readonly AppDbContext _context;

    public WalletService(ILogger<WalletService> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest request)
    {
        _logger.LogInformation("Creating wallet for {FirstName} {LastName}", request.FirstName, request.LastName);
        try
        {
            var customerId = Guid.NewGuid().ToString("N");

            var wallet = new Wallet
            {
                CustomerId = customerId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Wallet created with CustomerId {CustomerId}", customerId);

            return new CreateWalletResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Wallet created successfully"
                },
                AccountDetails = new AccountDetails
                {
                    CustomerId = customerId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wallet");
            return new CreateWalletResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message
                }
            };
        }
    }

    public async Task<CreditWalletResponse> CreditWalletAsync(CreditWalletRequest request)
    {
        _logger.LogInformation("Crediting wallet for {CustomerId}", request.CustomerId);
        try
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.CustomerId == request.CustomerId);

            if (wallet == null)
            {
                return new CreditWalletResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Wallet not found"
                    }
                };
            }

            wallet.Balance += request.Amount;

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString("N"),
                WalletId = wallet.Id,
                Amount = request.Amount,
                BalanceAfter = wallet.Balance,
                Description = request.Description ?? "Credit",
                Type = "CREDIT",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Wallet credited successfully for {CustomerId}", request.CustomerId);

            return new CreditWalletResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Wallet credited successfully"
                },
                TransactionId = transaction.TransactionId,
                Amount = request.Amount,
                Balance = wallet.Balance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crediting wallet");
            return new CreditWalletResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message
                }
            };
        }
    }

    public async Task<DebitWalletResponse> DebitWalletAsync(DebitWalletRequest request)
    {
        _logger.LogInformation("Debiting wallet for {CustomerId}", request.CustomerId);
        try
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.CustomerId == request.CustomerId);

            if (wallet == null)
            {
                return new DebitWalletResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Wallet not found"
                    }
                };
            }

            if (wallet.Balance < request.Amount)
            {
                return new DebitWalletResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "02",
                        ResponseMessage = "Insufficient balance"
                    }
                };
            }

            wallet.Balance -= request.Amount;

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString("N"),
                WalletId = wallet.Id,
                Amount = request.Amount,
                BalanceAfter = wallet.Balance,
                Description = request.Description ?? "Debit",
                Type = "DEBIT",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Wallet debited successfully for {CustomerId}", request.CustomerId);

            return new DebitWalletResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Wallet debited successfully"
                },
                TransactionId = transaction.TransactionId,
                Amount = request.Amount,
                Balance = wallet.Balance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debiting wallet");
            return new DebitWalletResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message
                }
            };
        }
    }

    public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request)
    {
        _logger.LogInformation("Getting balance for {CustomerId}", request.CustomerId);
        try
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.CustomerId == request.CustomerId);

            if (wallet == null)
            {
                return new GetBalanceResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Wallet not found"
                    }
                };
            }

            return new GetBalanceResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Balance retrieved successfully"
                },
                Balance = wallet.Balance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance");
            return new GetBalanceResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message
                }
            };
        }
    }

    public async Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest request)
    {
        _logger.LogInformation("Getting transaction {TransactionId}", request.TransactionId);
        try
        {
            var transaction = await _context.WalletTransactions
                .FirstOrDefaultAsync(t => t.TransactionId == request.TransactionId);

            if (transaction == null)
            {
                return new GetTransactionResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Transaction not found"
                    }
                };
            }

            return new GetTransactionResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Transaction retrieved successfully"
                },
                TransactionDetails = new TransactionDetails
                {
                    TransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    TranType = transaction.Type,
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction");
            return new GetTransactionResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message
                }
            };
        }
    }

    public async Task<RefundResponse> RefundAsync(RefundRequest request)
    {
        _logger.LogInformation("Refund for transaction {TransactionId}", request.TransactionId);
        try
        {
            var transaction = await _context.WalletTransactions
                .Include(t => t.Wallet)
                .FirstOrDefaultAsync(t => t.TransactionId == request.TransactionId);

            if (transaction == null)
            {
                return new RefundResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Transaction not found"
                    }
                };
            }

            if (transaction.Type != "DEBIT")
            {
                return new RefundResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "03",
                        ResponseMessage = "Only debit transactions can be refunded"
                    }
                };
            }

            transaction.Wallet.Balance += transaction.Amount;

            var refundTransaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid().ToString("N"),
                WalletId = transaction.WalletId,
                Amount = transaction.Amount,
                BalanceAfter = transaction.Wallet.Balance,
                Description = $"Refund for {transaction.TransactionId}",
                Type = "REFUND",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(refundTransaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refund successful for transaction {TransactionId}", request.TransactionId);

            return new RefundResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Refund successful"
                },
                Amount = transaction.Amount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund");
            return new RefundResponse
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message
                }
            };
        }
    }
}