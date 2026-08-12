using System.Net;
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
    
    public async Task<NameEnquiryWrapper> AccountEnquiry(string accountNumber, string bankCode)
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
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Name Enquiry returned {status}", response.StatusCode);
                var errorWrapper = JsonConvert.DeserializeObject<NameEnquiryWrapper>(rawContent);
                return errorWrapper!;
            }

            var wrapper = JsonConvert.DeserializeObject<NameEnquiryWrapper>(rawContent);

            _logger.LogInformation("Name Enquiry Successful for account {account}", wrapper!.Data!.AccountName);
            return wrapper;
        }
        catch (Exception ex)
        {
            _logger.LogError("wtf happened");
            return NameEnquiryError(ex.Message);
        }
    }
    
    private static NameEnquiryWrapper NameEnquiryError(string message) =>
        new ()
        {
            Data = null,
            Status = false,
            Message = message
        };
    
    
    private static string GenerateSessionId()
    {
        var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
        var random = Random.Shared.Next(100_000_000, 999_999_999).ToString("D12")[..12];
        var sessionId = $"{InstitutionId}{timestamp}{random}";
        return sessionId;
    }
}

