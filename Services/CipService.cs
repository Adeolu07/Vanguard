using System.Net;
using System.Text;
using Tripfinity.Interfaces;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;
using Newtonsoft.Json;

namespace Tripfinity.Services;

public class CipService : ICipService
{
    private readonly HttpClient _client;
    private readonly ILogger<CipService> _logger;
    private readonly string _institutionId;
    private readonly string _senderAccountNumber;

    // Operational identifiers. They are not secrets, but they are environment
    // specific, so they can be overridden through configuration.
    private const string DefaultInstitutionId = "000966";
    private const string DefaultSenderAccountNumber = "4275719875";

    public CipService(HttpClient client, ILogger<CipService> logger, IConfiguration configuration)
    {
        _client = client;
        _logger = logger;
        _institutionId = configuration["Cip:InstitutionId"] ?? DefaultInstitutionId;
        _senderAccountNumber = configuration["Cip:SenderAccountNumber"] ?? DefaultSenderAccountNumber;
    }
    
    public async Task<NameEnquiryWrapper> AccountEnquiry(string accountNumber, string bankCode)
    {
        try
        {
            var request = new NameEnquiryRequest
            {
                SessionId = GenerateSessionId(),
                SenderAccountNumber = _senderAccountNumber,
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
                _logger.LogWarning("Name Enquiry returned {status}", (int)response.StatusCode);
                return NameEnquiryError($"Provider returned HTTP {(int)response.StatusCode}.");
            }

            var wrapper = JsonConvert.DeserializeObject<NameEnquiryWrapper>(rawContent);

            if(wrapper?.Data == null)
                return NameEnquiryError("Empty response from provider");

            if (wrapper.Data.ResponseCode != "00")
                return new NameEnquiryWrapper
                {
                    Data = wrapper.Data,
                    Status = false,
                    Message = wrapper.Data.ResponseMessage
                };
            
            _logger.LogInformation("Name enquiry successful for account ending {Suffix}",
                accountNumber.Length > 4 ? accountNumber[^4..] : "****");
            return wrapper;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Account enquiry failed for account ending {Suffix}",
                accountNumber.Length > 4 ? accountNumber[^4..] : "****");
            return NameEnquiryError("Could not reach the account enquiry service.");
        }
    }
    
    private static NameEnquiryWrapper NameEnquiryError(string message) =>
        new ()
        {
            Data = null,
            Status = false,
            Message = message
        };
    
    
    private string GenerateSessionId()
    {
        var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
        var random = Random.Shared.Next(100_000_000, 999_999_999).ToString("D12")[..12];
        var sessionId = $"{_institutionId}{timestamp}{random}";
        return sessionId;
    }
}

