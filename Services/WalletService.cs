using System.Net.Http.Headers;
using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
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
                // Cache the token until its expiry (default 1 hour if not provided)
                var expiry = DateTime.TryParse(result.ExpiryDate, out var expiryDate);
                AuthToken token = new AuthToken
                {
                    ExpiryDate = DateTime.Parse(result.ExpiryDate),
                    Token = result.Token!,
                };
            
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Token);
            
                _context.AuthTokens.Add(token);
                await _context.SaveChangesAsync();
                _cache.Set(TokenCacheKey, result.Token, expiryDate);
                _logger.LogInformation("Wallet token acquired and cached");
                
            }
        }
        else
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken.Token);
            _cache.Set(TokenCacheKey,authToken.Token, authToken.ExpiryDate);
        }
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
            _logger.LogInformation(result.ResponseHeader.ResponseMessage);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<CreditWalletResponse> CreditWalletAsync(CreditWalletRequest creditWallet)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Crediting Wallet...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(creditWallet);
            
            var response = await _client.PostAsync(
                "Credit", 
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<CreditWalletResponse>(error);
                _logger.LogWarning("Wallet credit error: " + errorJson);
                return errorJson;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CreditWalletResponse>(json);
            
            if(result.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully credited wallet");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
        
    }

    public async Task<DebitWalletResponse> DebitWalletAsync(DebitWalletRequest debitWallet)
    {
        await EnsureAuthenticatedAsync();
        _logger.LogInformation("Debit Wallet...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(debitWallet);
            var response = await _client.PostAsync(
                "Debit",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<DebitWalletResponse>(error);
                _logger.LogWarning("Wallet debit error: " + errorJson);
                return errorJson;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DebitWalletResponse>(json);
            
            if(result.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully debited wallet");
            
            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
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
                _logger.LogWarning("Error in Fetching Balance: " + errorJson);
                return errorJson;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetBalanceResponse>(json);
            
            if(result.ResponseHeader.ResponseCode == "00")
                _logger.LogInformation("Successfully fetched balance");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
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
                return errorJson;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetTransactionResponse>(json);
            
            if(result.ResponseHeader.ResponseCode == "00")
            {
                _logger.LogInformation("Successfully fetched transaction with ID {TransactionID}", result.TransactionDetails.TransactionId);
            }
            
            return result;

        }
        catch (Exception ex)
        { 
            _logger.LogError(ex.Message);
            throw;
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
                return errorJson;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RefundResponse>(json);
            
            if(result.ResponseHeader.ResponseCode == "00")
            {
                _logger.LogInformation("Successfull refund");
            }
            return result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }
    
}