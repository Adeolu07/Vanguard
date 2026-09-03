using System.Net;
using System.Net.Http.Headers;
using System.Text;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Utilities;
using Newtonsoft.Json;

namespace _Tripfinity.Services;

public class FastChannelService : IFastChannelService
{
    private readonly HttpClient _client;
    private readonly ILogger<FastChannelService> _logger;
    private readonly IConfiguration _config;
    private readonly ExternalTokenStore _tokenStore;

    public FastChannelService(
        HttpClient client,
        ILogger<FastChannelService> logger,
        IConfiguration config,
        ExternalTokenStore tokenStore)
    {
        _client = client;
        _logger = logger;
        _config = config;
        _tokenStore = tokenStore;
    }

    /// <summary>
    /// Ensures a valid bearer token for the configured FastChannel
    /// integration credential is installed on the outgoing HTTP client.
    /// </summary>
    private async Task EnsureAuthenticatedAsync()
    {
        var token = await _tokenStore.GetTokenAsync(
            ExternalTokenStore.Providers.FastChannel,
            AcquireTokenAsync);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Calls the FastChannel Authentication endpoint using the client
    /// credential stored in configuration. Used when no unexpired token
    /// is found in cache or in the token store table.
    /// </summary>
    private async Task<(string Token, DateTime ExpiryDate)> AcquireTokenAsync()
    {
        var authRequest = new FcAuthReq
        {
            Username = _config["FastChannel:Username"]!,
            Password = _config["FastChannel:Password"]!
        };

        var json = JsonConvert.SerializeObject(authRequest);
        var res = await _client.PostAsync("Authentication",
            new StringContent(json, Encoding.UTF8, "application/json"));
        var rawContent = await res.Content.ReadAsStringAsync();

        _logger.LogInformation("FastChannel authentication response [Status={StatusCode}]",
            (int)res.StatusCode);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("FastChannel authentication failed [Status={StatusCode}]: {RawContent}",
                (int)res.StatusCode, rawContent);
            throw new InvalidOperationException("FastChannel authentication failed.");
        }

        var parsed = JsonConvert.DeserializeObject<FcAuthRes>(rawContent);

        if (parsed?.ResponseHeader.ResponseCode != "00" || string.IsNullOrWhiteSpace(parsed.Token))
        {
            _logger.LogError("FastChannel authentication unsuccessful: {ResponseCode} {ResponseMessage}",
                parsed?.ResponseHeader.ResponseCode, parsed?.ResponseHeader.ResponseMessage);
            throw new InvalidOperationException("FastChannel authentication not successful.");
        }

        var expiry = DateTime.TryParse(parsed.ExpiryDate, out var expiryDate)
            ? expiryDate
            : DateTime.Now.AddHours(1);

        return (parsed.Token, expiry);
    }

    public async Task<(HttpStatusCode StatusCode, FcAuthRes? Response)> Authentication(FcAuthReq request)
    {
        try
        {
            var req = JsonConvert.SerializeObject(request);
            var res = await _client.PostAsync("Authentication",
                new StringContent(req, Encoding.UTF8, "application/json"));
            var rawContent = await res.Content.ReadAsStringAsync();
            _logger.LogInformation("Authentication response [Status={StatusCode}]: {RawContent}",
                (int)res.StatusCode, rawContent);

            var response = JsonConvert.DeserializeObject<FcAuthRes>(rawContent);

            if (response is null)
            {
                _logger.LogError("Failed to deserialize authentication response: {RawContent}", rawContent);
                return (res.StatusCode, FailedAuth(
                    string.IsNullOrWhiteSpace(rawContent)
                        ? $"FastChannel returned HTTP {(int)res.StatusCode}."
                        : rawContent));
            }

            _logger.LogInformation("FastChannel authentication successful");
            return (res.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FastChannel authentication error");
            return (HttpStatusCode.BadGateway, FailedAuth(ex.Message));
        }
    }

    public async Task<(HttpStatusCode StatusCode, FcSinglePostRes? Response)> SinglePostAsync(FcSinglePostReq request)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var req = JsonConvert.SerializeObject(request);
            var res = await _client.PostAsync("SinglePost",
                new StringContent(req, Encoding.UTF8, "application/json"));
            var rawContent = await res.Content.ReadAsStringAsync();
            _logger.LogInformation("Single Post response [Status={StatusCode}]:",
                (int)res.StatusCode);

            var response = JsonConvert.DeserializeObject<FcSinglePostRes>(rawContent);

            if (response is null)
            {
                _logger.LogError("Failed to deserialize Single Post response: {RawContent}", rawContent);
                return (res.StatusCode, FailedSinglePost(
                    string.IsNullOrWhiteSpace(rawContent)
                        ? $"FastChannel returned HTTP {(int)res.StatusCode}."
                        : rawContent));
            }

            _logger.LogInformation("Single Post processed");
            return (res.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Single Post error");
            return (HttpStatusCode.BadGateway, FailedSinglePost(ex.Message));
        }
    }

    private static FcAuthRes FailedAuth(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            Key = null,
            Token = null,
            ExpiryDate = null
        };

    private static FcSinglePostRes FailedSinglePost(string message) =>
        new()
        {
            ResponseHeader = new ResponseHeader { ResponseCode = "99", ResponseMessage = message },
            TraceId = null,
            BatchId = null
        };
}