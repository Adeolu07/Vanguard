using System.Net.Http.Headers;
using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using Newtonsoft.Json;

namespace _Tripfinity.Services;

public class WalletService : IWalletService
{
    private readonly HttpClient _client;
    private readonly ILogger<WalletService> _logger;
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public WalletService( HttpClient client,
        ILogger<WalletService> logger,
        AppDbContext context, IConfiguration config)
    {
        _logger = logger;
        _client = client;
        _context = context;
        _config = config;
        AuthenticateOnStartup().GetAwaiter().GetResult();
    }

    private async Task AuthenticateOnStartup()
    {
        var existing = _context.WalletTokens.FirstOrDefault();
        if (existing != null && existing.ExpiryDate > DateTime.Now)
        {
            _client.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Bearer", existing.Token);
            _logger.LogInformation("Token loaded from DB");
            return;
        }
        
        var authRequest = new AuthenticationRequest
        {
            Username = _config["WalletStation:username"],
            Password = _config["WalletStation:password"]
        };
    
        await AuthenticationAsync(authRequest);
    }
    
    public async Task<AuthenticationResponse> AuthenticationAsync(AuthenticationRequest authenticationRequest)
    {
        _logger.LogInformation("Authenticating user...");
        var requestBody = JsonConvert.SerializeObject(authenticationRequest);
        var response = await _client.PostAsync("Auth",
            new StringContent(requestBody, Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<AuthenticationResponse>(error);
                _logger.LogWarning("Error in Authenticate: " + errorJson);
                return errorJson;
        }
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<AuthenticationResponse>(json); ;
        
        if (result.ResponseHeader.ResponseCode == "00")
        {
            _logger.LogInformation("Created brand new wallet btw");
           var existing = _context.WalletTokens.FirstOrDefault();
           if (existing != null)
           {
               _context.WalletTokens.Remove(existing);
           }
           DateTime.TryParse(result.ExpiryDate, out DateTime ExpiryDate);
           _context.WalletTokens.Add(new WalletToken
           {
               Token = result.Token,
               ExpiryDate = ExpiryDate,
           });
           await _context.SaveChangesAsync();
           _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        }
        return result;
        
    }
    
    public async Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest createWallet)
    {
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
                return errorJson;
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