using System.Net.Http.Headers;
using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace _Tripfinity.Services;

public class WalletService : IWalletService
{
    private readonly HttpClient _client;
    private readonly ILogger<WalletService> _logger;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _context;
    private const string TokenCacheKey = "WalletToken";

    public WalletService( HttpClient client,
        ILogger<WalletService> logger, IConfiguration config
        ,AppDbContext context, IMemoryCache cache)
    {
        _logger = logger;
        _client = client;
        _config = config;
        _cache = cache;
        _context = context;
    }

    public async Task EnsureAuthenticatedAsync()
    {
        // Check cache first
        if (_cache.TryGetValue<string>(TokenCacheKey, out var cachedToken))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", cachedToken);
            _logger.LogInformation("Wallet token found in cache");
            return;
        }

        var authToken = _context.AuthTokens.FirstOrDefault(t => t.ExpiryDate > DateTime.Now);

        if (authToken == null)
        {
            var authRequest = new AuthenticationRequest
            {
                Username = _config["WalletStation:username"]!,
                Password = _config["WalletStation:password"]!,
            };

            var requestBody = JsonConvert.SerializeObject(authRequest);
            var response = await _client.PostAsync("Auth",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Wallet auth failed: {Error}", error);
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthenticationResponse>(json);
        

            if (result?.ResponseHeader.ResponseCode == "00")
            {
                var _ = DateTime.TryParse(result.ExpiryDate, out var expiryDate);
                AuthToken token = new AuthToken
                {
                    ExpiryDate = DateTime.Parse(result.ExpiryDate),
                    Token = result.Token!,
                };
            
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Token);
                _context.AuthTokens.Update(token);
                await _context.SaveChangesAsync();
                
                _cache.Set(TokenCacheKey, result.Token, expiryDate);
                _logger.LogInformation("Wallet token acquired and cached");
                
            }
        }
        else
        {
            _logger.LogInformation("Wallet token acquired from DB");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken.Token);
            _cache.Set(TokenCacheKey,authToken.Token, authToken.ExpiryDate);
        }
    }
    
    private async Task LogTransaction(WalletTransaction data, string type, string customerId)
    {
        if (data.ResponseHeader.ResponseCode != "00")
        {
            return;
        }
        
        var user = _context.Users.FirstOrDefault(u => u.UserWalletId == customerId);
        if (user == null)
        {
            _logger.LogWarning("Wallet transaction logged without user mapping for CustomerId {CustomerId}",
                customerId);
            return;
        }

        _context.Transactions.Add(new Transaction
        {
            UserId = user.Id,
            Type = type,
            Amount = data.Amount,
            Currency = "NGN",
            Description = data.Description,
            ExternalReference = data.TransactionId,
            InternalReference = data.TraceId,
            Status = "Completed",
            CreatedAt = DateTime.Now,
            CompletedAt = DateTime.Now
        });
        await _context.SaveChangesAsync();
    }
    
    public async Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest createWallet)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Creating Wallet...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(createWallet);
            var response = await _client.PostAsync(
                "CreateAccount", 
                new StringContent(
                    requestBody,
                    Encoding.UTF8,
                    "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<CreateWalletResponse>(error);
                _logger.LogWarning("Wallet creation error: " + errorJson);
                return errorJson!;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CreateWalletResponse>(json);
            _logger.LogInformation("Wallet endpoint hit successfully");
            return result!;
        }
        catch (Exception ex)
        {
            return FailedCreateWallet(ex.Message);
        }
    }

    public async Task<WalletTransaction> CreditWalletAsync(CreditWalletRequest creditWallet)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Crediting Wallet...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(creditWallet);
            
            var response = await _client.PostAsync("Credit", 
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<WalletTransaction>(error);
                _logger.LogWarning("Wallet credit error: " + errorJson);
                return errorJson!;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<WalletTransaction>(json);
            
            if(result!.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully credited wallet");

            await LogTransaction(result, "Credit", creditWallet.CustomerId!);
            
            return result;
        }
        catch (Exception ex)
        {
            return FailedTransaction(ex.Message);
        }
        
    }

    public async Task<WalletTransaction> DebitWalletAsync(DebitWalletRequest debitWallet)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Debit Wallet...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(debitWallet);
            var response = await _client.PostAsync("Debit",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<WalletTransaction>(error);
                _logger.LogWarning("Wallet debit error: " + errorJson);
                return errorJson!;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<WalletTransaction>(json);
            
            if(result!.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully debited wallet");

            await LogTransaction(result, "Debit", debitWallet.CustomerId);
            
            return result;

        }
        catch (Exception ex)
        {
            return FailedTransaction(ex.Message);
        }
    }

    public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest getBalance)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Get Balance...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(getBalance);
            
            var response = await _client.PostAsync("GetBalance",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));
            
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<GetBalanceResponse>(error);
                _logger.LogError("Error in Fetching Balance: {error}", errorJson);
                return errorJson!;
            }
            var successfulResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetBalanceResponse>(successfulResponse);
            
            _logger.LogInformation(result!.ResponseHeader.ResponseMessage);
            if(result.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully fetched balance");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            return FailedGetBalance(ex.Message);
        }
    }
    
    public async Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest getTransaction)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Get Transaction...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(getTransaction);

            var response = await _client.PostAsync("GetTransaction",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<GetTransactionResponse>(error);
                _logger.LogWarning("Error in Fetching Transaction: " + errorJson);
                return errorJson!;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetTransactionResponse>(json);
            
            if(result!.ResponseHeader.ResponseCode == "00")
            {
                _logger.LogInformation("Successfully fetched transaction with ID {TransactionID}", result.TransactionDetails!.TransactionId);
            }
            
            return result;

        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "GetTransaction failed for {TransactionId}", getTransaction);
            return FailedGetTransaction(ex.Message);
        }
    }

    public async Task<RefundResponse> RefundAsync(RefundRequest refund)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Refund initiated");
        try
        {
            var requestBody = JsonConvert.SerializeObject(refund);

            var response = await _client.PostAsync("DebitReversal",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<RefundResponse>(error);
                _logger.LogWarning("Error in Refund: " + errorJson);
                return errorJson!;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RefundResponse>(json);
            
            if(result!.ResponseHeader.ResponseCode == "00")
            {
                _logger.LogInformation("Successful refund");
                var localTransaction = _context.Transactions
                    .FirstOrDefault(transaction => transaction.ExternalReference == refund.TransactionId
                                                   && transaction.Type == "Debit");

                var originalTransaction = await GetTransactionAsync(new GetTransactionRequest
                {
                    TransactionId = refund.TransactionId
                });
                
                var amount = originalTransaction.TransactionDetails?.Amount ?? localTransaction?.Amount ?? 0m;
                var userId = localTransaction?.UserId ?? 0;
                var description = localTransaction?.Description ?? refund.Description;


                _context.Transactions.Add(new Transaction
                {
                    UserId = userId,
                    Type = "Refund",
                    Amount = amount,
                    Currency = "NGN",
                    Description = $"Refund: {description}",
                    ExternalReference = refund.TransactionId,
                    InternalReference = localTransaction!.InternalReference ?? refund.TransactionId,
                    Status = "Completed",
                    CreatedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refund failed for {TransactionId}", refund.TransactionId);
            return FailedRefund(ex.Message);
        }
    }

    public async Task<GetTransactionListResponse> GetTransactionList(GetTransactionListRequest request)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Get Transaction List");

        try
        {
            var requestBody = JsonConvert.SerializeObject(request);

            var response = await _client.PostAsync("GetTransactionList",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<GetTransactionListResponse>(error);
                _logger.LogWarning("Error getting transaction list " + errorJson);
                return errorJson!;
            }

            var result = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<GetTransactionListResponse>(result);

            if (jsonResponse?.ResponseHeader.ResponseCode == "00")
            {
                _logger.LogInformation("Successful Get Transaction List");
                return jsonResponse;
            }
            _logger.LogInformation("Unsuccessful Get Transaction List");
            return jsonResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Get Transaction Failed for Customer {CustomerId}", request.CustomerId);
            return FailedGetTransactionList(ex.Message);
        }
    }

    public async Task<WalletNameEnquiryResponse> WalletNameEnquiryAsync(WalletNameEnquiryRequest request)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Name Enquiry...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(request);

            var response = await _client.PostAsync("NameEnquiry",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<WalletNameEnquiryResponse>(error);
                _logger.LogError("Error in Name Enquiry: {error}", errorJson);
                return errorJson!;
            }
            
            var successfulResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<WalletNameEnquiryResponse>(successfulResponse);
            
            _logger.LogInformation(result!.ResponseHeader.ResponseMessage);
            if(result.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfull Name Enquiry");

            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Name enquiry failed for Customer {CustomerId}", request.CustomerId);
            return FailedNameEnquiry(ex.Message);
        }
    }
    
    private static WalletNameEnquiryResponse FailedNameEnquiry(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            AccountNumber = null,
            FirstName = null,
            LastName = null,
            CustomerId = null,
            CustomerAlias = null,
            BankCode = null,
            BankName = null,
            Bvn = null,
        };

    private static GetTransactionListResponse FailedGetTransactionList(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            Pagination = null,
            TransactionDetailsList = null
        };
    
    private static WalletTransaction FailedTransaction(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            Amount       = 0,
            Balance      = 0,
            Description  = message,
            TransactionId = "",
            TraceId      = ""
        };
    private static GetBalanceResponse FailedGetBalance(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            Balance = 0
        };

    private static GetTransactionResponse FailedGetTransaction(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            TransactionDetails = null
        };

    private static RefundResponse FailedRefund(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            Amount = null
        };
    private static CreateWalletResponse FailedCreateWallet(string message) =>
        new ()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            AccountDetails = null!
        };
}