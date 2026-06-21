using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using Newtonsoft.Json;

namespace _Tripfinity.Services;

public class WalletService : IWalletService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _client;
    private readonly ISessionStore _session;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IConfiguration config, HttpClient client,
        ISessionStore session, ILogger<WalletService> logger)
    {
        _session = session;
        _logger = logger;
        _client = client;
        _config = config;
    }


    public async Task<AuthenticationResponse> AuthenticationAsync(AuthenticationRequest request)
    {
        _logger.LogInformation("Authenticating...");
        try
        {
            var requestBody = JsonConvert.SerializeObject(request);

            var response = await _client.PostAsync(
                "Auth",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<AuthenticationResponse>(error);
                _logger.LogWarning("Error in Wallet AuthService: " + errorJson);
                return errorJson;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthenticationResponse>(json);
            if (result.ResponseHeader.ResponseCode == "00")
            {
                _session.Set(result.Token);
                _logger.LogInformation("Token set");
            }
            
            _logger.LogInformation("Wallet service functional");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
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
            _logger.LogInformation("Wallet creation successful");
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
            var session = _session.Get();
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
            var session = _session.Get();
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
            var session = _session.Get();
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
            var session = _session.Get();
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
            var session = _session.Get();
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