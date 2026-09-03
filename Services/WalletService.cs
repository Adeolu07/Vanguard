using System.Net.Http.Headers;
using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace _Tripfinity.Services;

public class WalletService : IWalletService
{
    private readonly HttpClient _client;
    private readonly ILogger<WalletService> _logger;
    private readonly IConfiguration _config;
    private readonly ExternalTokenStore _tokenStore;
    private readonly AppDbContext _context;

    public WalletService(HttpClient client,
        ILogger<WalletService> logger, IConfiguration config,
        AppDbContext context, IMemoryCache cache,ExternalTokenStore tokenStore)
    {
        _logger = logger;
        _client = client;
        _config = config;
        _tokenStore = tokenStore;
        _context = context;
    }

    public async Task EnsureAuthenticatedAsync()
    {
        var token = await _tokenStore.GetTokenAsync(
            ExternalTokenStore.Providers.WalletStation,AcquireTokenAsync);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    private async Task<(string Token, DateTime ExpiryDate)> AcquireTokenAsync()
    {
        var authRequest = new AuthenticationRequest
        {
            Username = _config["WalletStation:Username"]!,
            Password = _config["WalletStation:Password"]!,
        };

        var requestBody = JsonConvert.SerializeObject(authRequest);
        _logger.LogInformation("Wallet Auth request: {Request}", requestBody.Substring(0, 
            Math.Min(requestBody.Length, 200)));

        var response = await _client.PostAsync("Auth",
            new StringContent(requestBody, Encoding.UTF8, "application/json"));

        var rawContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Wallet Auth response [Status={StatusCode}]",
            (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Wallet auth failed: {RawContent}", rawContent);
            throw new InvalidOperationException("Unable to authenticate to wallet service.");
        }

        var result = JsonConvert.DeserializeObject<AuthenticationResponse>(rawContent);

        if (result?.ResponseHeader.ResponseCode == "00" && string.IsNullOrWhiteSpace(result.Token))
        {
            _logger.LogError("Wallet auth returned success but token was empty.");
            throw new InvalidOperationException("Wallet authentication token missing.");
        }

        var expiryDate = DateTime.Parse(result!.ExpiryDate);
        _logger.LogInformation("Wallet token acquired, valid until {Expiry}", expiryDate);

        return (result.Token!, expiryDate);
    }

    public async Task<MarshalWalletViewModel> BuildWalletInfoAsync(string walletId, int page)
    {
        var response = await GetBalanceAsync(new GetBalanceRequest
        {
            CustomerId = walletId
        });

        var balance = response.Balance;
        
        var transactions = new List<TransactionDetailsList>();
        var totalPages = 1;
        var hasNext = false;
        var hasPrev = false;

        var listResponse = await GetTransactionList(new GetTransactionListRequest
        {
            CustomerId = walletId,
            SearchDetails = new SearchDetails
            {
                Page = page,
                ItemsPerPage = 10,
                DateRange = new DateRange { Start = DateTime.Now.AddMonths(-3), End = DateTime.Now }
            }
        });

        if (listResponse.TransactionDetailsList != null)
        { 
            transactions = listResponse.TransactionDetailsList
                .Select(d => new TransactionDetailsList
                {
                    TranType = d.TranType,
                    Amount = d.Amount,
                    Description = d.Description,
                    TransactionId = d.TransactionId,
                    SessionId = d.SessionId
                }).ToList();

            totalPages = listResponse.Pagination?.TotalPages ?? 1;
            hasNext = listResponse.Pagination?.HasNext ?? false;
            hasPrev = listResponse.Pagination?.HasPrevious ?? false;
        }

        return new MarshalWalletViewModel
        {
            WalletId = walletId,
            Balance = balance,
            Transactions = transactions,
            CurrentPage = page,
            TotalPages = totalPages,
            HasNext = hasNext,
            HasPrevious = hasPrev
        };

    }

    private async Task LogTransaction(WalletTransaction data, string type, string customerId)
    {
        if (data.ResponseHeader.ResponseCode != "00") return;

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
            _logger.LogInformation("Wallet CreateAccount request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("CreateAccount",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet CreateAccount response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<CreateWalletResponse>(rawContent);
                _logger.LogWarning("Wallet creation error: {@Error}", errorJson);
                return errorJson!;
            }

            var result = JsonConvert.DeserializeObject<CreateWalletResponse>(rawContent);
            _logger.LogInformation("Wallet created successfully: {CustomerId}", result?.AccountDetails.CustomerId);
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWallet failed");
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
            _logger.LogInformation("Wallet Credit request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("Credit",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet Credit response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<WalletTransaction>(rawContent);
                _logger.LogWarning("Wallet credit error: {@Error}", errorJson);
                return errorJson!;
            }

            var result = JsonConvert.DeserializeObject<WalletTransaction>(rawContent);

            if (result!.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully credited wallet. New balance: {Balance}", result.Balance);

            await LogTransaction(result, "Credit", creditWallet.CustomerId!);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreditWallet failed");
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
            _logger.LogInformation("Wallet Debit request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("Debit",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet Debit response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<WalletTransaction>(rawContent);
                _logger.LogWarning("Wallet debit error: {@Error}", errorJson);
                return errorJson!;
            }

            var result = JsonConvert.DeserializeObject<WalletTransaction>(rawContent);

            if (result!.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully debited wallet");

            await LogTransaction(result, "Debit", debitWallet.CustomerId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DebitWallet failed");
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
            _logger.LogInformation("Wallet GetBalance request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("GetBalance",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet GetBalance response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<GetBalanceResponse>(rawContent);
                _logger.LogError("Error in Fetching Balance: {@Error}", errorJson);
                return errorJson!;
            }

            var result = JsonConvert.DeserializeObject<GetBalanceResponse>(rawContent);

            if (result!.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully fetched balance: {Balance}", result.Balance);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetBalance failed");
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
            _logger.LogInformation("Wallet GetTransaction request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("GetTransaction",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet GetTransaction response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<GetTransactionResponse>(rawContent);
                _logger.LogWarning("Error in Fetching Transaction: {@Error}", errorJson);
                return errorJson!;
            }

            var result = JsonConvert.DeserializeObject<GetTransactionResponse>(rawContent);

            if (result!.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully fetched transaction {TransactionId}",
                    result.TransactionDetails!.TransactionId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTransaction failed for {TransactionId}", getTransaction.TransactionId);
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
            _logger.LogInformation("Wallet DebitReversal request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("DebitReversal",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet DebitReversal response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<RefundResponse>(rawContent);
                _logger.LogWarning("Error in Refund: {@Error}", errorJson);
                return errorJson!;
            }

            var result = JsonConvert.DeserializeObject<RefundResponse>(rawContent);

            if (result!.ResponseHeader.ResponseCode == "00")
            {
                _logger.LogInformation("Successful refund");

                var localTransaction = _context.Transactions
                    .FirstOrDefault(t => t.ExternalReference == refund.TransactionId && t.Type == "Debit");

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
            _logger.LogInformation("Wallet GetTransactionList request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("GetTransactionList",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet GetTransactionList response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<GetTransactionListResponse>(rawContent);
                _logger.LogWarning("Error getting transaction list: {@Error}", errorJson);
                return errorJson!;
            }

            var jsonResponse = JsonConvert.DeserializeObject<GetTransactionListResponse>(rawContent);

            if (jsonResponse?.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successful Get Transaction List");
            else
                _logger.LogInformation("Unsuccessful Get Transaction List");

            return jsonResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTransactionList failed for Customer {CustomerId}", request.CustomerId);
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
            _logger.LogInformation("Wallet NameEnquiry request: {RequestBody}", requestBody);

            var response = await _client.PostAsync("NameEnquiry",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wallet NameEnquiry response [Status={StatusCode}]: {RawContent}",
                (int)response.StatusCode, rawContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = JsonConvert.DeserializeObject<WalletNameEnquiryResponse>(rawContent);
                _logger.LogError("Error in Name Enquiry: {@Error}", errorJson);
                return errorJson!;
            }

            var result = JsonConvert.DeserializeObject<WalletNameEnquiryResponse>(rawContent);

            if (result!.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successful Name Enquiry");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Name enquiry failed for Customer {CustomerId}", request.CustomerId);
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
            Amount = 0,
            Balance = 0,
            Description = message,
            TransactionId = "",
            TraceId = ""
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
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            AccountDetails = null!
        };
}