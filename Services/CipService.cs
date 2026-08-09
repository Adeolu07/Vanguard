using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using Newtonsoft.Json;

namespace _Tripfinity.Services;

public class CipService : ICipService
{
    private readonly HttpClient _client;
    private readonly ILogger<CipService> _logger;

    private const string InstitutionId = "000966";
    private const string SenderAccountNumber = "4275719875";
    public CipService(HttpClient client, ILogger<CipService> logger)
    {
        _client = client;
        _logger = logger;
    }
    
    public async Task<NameEnquiryResponse> AccountEnquiry(string accountNumber, string bankCode)
    {
        try
        {
            var request = new NameEnquiryRequest
            {
                SessionId = GenerateSessionId(),
                SenderAccountNumber = SenderAccountNumber,
                DestinationInstitutionCode = bankCode,
                DestinationAccountNumber = accountNumber
            };

            var requestBody = JsonConvert.SerializeObject(request);
            var response = await _client.PostAsync(
                "AccountEnquiry",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("CIP raw response: {raw}", rawContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Name Enquiry returned {status}:{raw}", response.StatusCode, rawContent);
                var errorWrapper = JsonConvert.DeserializeObject<NameEnquiryWrapper>(rawContent);
                return errorWrapper?.Data ?? NameEnquiryError($"Unexpected error: {rawContent}");
            }

            var wrapper = JsonConvert.DeserializeObject<NameEnquiryWrapper>(rawContent);
            if (wrapper?.Data == null)
            {
                _logger.LogWarning("Deserialized NameEnquiry returned null. Raw: {raw}", rawContent);
                return NameEnquiryError($"Missing data in response: {rawContent}");
            }

            _logger.LogInformation("Name Enquiry Successful for account {account}", wrapper.Data.AccountNumber);
            return wrapper.Data;
        }
        catch (Exception ex)
        {
            return NameEnquiryError(ex.Message);
        }
    }

    public async Task<PostCreditResponse> PostCredit(decimal amount, string accountNumber, string accountName, string bankCode)
    {
        try
        {
            var sessionId = GenerateSessionId();
            var request = new PostCreditRequest
            {
                SessionId = sessionId,
                PaymentReference = sessionId,
                Amount = amount,
                Channel = "USSD",
                DestinationAccountName = accountName,
                DestinationAccountNumber = accountNumber,
                DestinationInstitutionCode = bankCode,
                Group = "Cooperate",
                Narration = "Cashout",
                Sector = "Banking",
                SenderAccountNumber = SenderAccountNumber
            };
            
            var requestBody = JsonConvert.SerializeObject(request);
            var response = await _client.PostAsync(
                "transaction/ContinueTransfer",   // ← double‑check this endpoint name with your CIP provider
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var rawContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("CIP raw response: {raw}", rawContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PostCredit returned {status}:{raw}", response.StatusCode, rawContent);
                var errorWrapper = JsonConvert.DeserializeObject<PostCreditWrapper>(rawContent);
                return errorWrapper?.Data ?? PostCreditError($"Unexpected error: {rawContent}");
            }

            var wrapper = JsonConvert.DeserializeObject<PostCreditWrapper>(rawContent);
            if (wrapper?.Data == null)
            {
                _logger.LogWarning("Deserialized PostCredit returned null. Raw: {raw}", rawContent);
                return PostCreditError($"Missing data in response: {rawContent}");
            }

            _logger.LogInformation("PostCredit Successful. ResponseCode: {code}", wrapper.Data.ResponseCode);
            return wrapper.Data;
        }
        catch (Exception ex)
        {
            return PostCreditError(ex.Message);
        }
        
    }

    public async Task<TransactionQueryResponse> TransactionQuery(TransactionQueryRequest tsqRequest)
    {
        try
        {
            var requestBody = JsonConvert.SerializeObject(tsqRequest);
            var response = await _client.PostAsync(
                "TransactionQuery", 
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<TransactionQueryResponse>(error);
                _logger.LogWarning("TSQ failed: " + errorJson);
                return errorJson!;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TransactionQueryResponse>(json);
            _logger.LogInformation("TSQ successful");
            return result!;
            

        }
        catch (Exception ex)
        {
            return TransactionQueryError(ex.Message);
        }
    }
    
    private static NameEnquiryResponse NameEnquiryError(string message) =>
        new ()
        {
            ResponseCode = "99",
            ResponseMessage = message
        };
    
    private static PostCreditResponse PostCreditError(string message) =>
        new ()
        {
            ResponseCode = "99",
            ResponseMessage = message
        };
    
    private static TransactionQueryResponse TransactionQueryError(string message) =>
        new ()
        {
            SessionId = "",
            PaymentRef = "",
            ResponseCode = "99",
            ResponseMessage = message
        };
    
    private static string GenerateSessionId()
    {
        var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
        var random = Random.Shared.Next(100_000_000, 999_999_999).ToString("D12")[..12];
        var sessionId = $"{InstitutionId}{timestamp}{random}";
        return sessionId;
    }
}

