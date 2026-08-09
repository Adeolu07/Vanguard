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

    public CipService(HttpClient client, ILogger<CipService> logger)
    {
        _client = client;
        _logger = logger;
    }
    public async Task<NameEnquiryResponse> AccountEnquiry(NameEnquiryRequest request)
    {
        try
        {
            var requestBody = JsonConvert.SerializeObject(request);
            var response = await _client.PostAsync(
                "nameenquiry", 
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<NameEnquiryResponse>(error);
                _logger.LogWarning("Name Enquiry Failed: " + errorJson);
                return errorJson!;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NameEnquiryResponse>(json);
            _logger.LogInformation("Name Enquiry Successful");
            return result!;
        }
        catch (Exception ex)
        {
            return NameEnquiryError(ex.Message);
        }
    }

    public async Task<PostCreditResponse> PostCredit(PostCreditRequest pcRequest)
    {
        try
        {
            var requestBody = JsonConvert.SerializeObject(pcRequest);
            var response = await _client.PostAsync(
                "postCredit", 
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var errorJson = JsonConvert.DeserializeObject<PostCreditResponse>(error);
                _logger.LogWarning("Fund Transfer Failed: " + errorJson);
                return errorJson!;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PostCreditResponse>(json);
            _logger.LogInformation("Fund Transfer Successful");
            return result!;
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
                "postCredit", 
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
    
    private static PostCreditResponse PostCreditError(string responseMessage) =>
        new ()
        {
            SessionId = "",
            PaymentRef = "",
            ResponseCode = "99",
            ResponseMessage = responseMessage
        };
    
    private static TransactionQueryResponse TransactionQueryError(string responseMessage) =>
        new ()
        {
            SessionId = "",
            PaymentRef = "",
            ResponseCode = "99",
            ResponseMessage = responseMessage
        };
}

