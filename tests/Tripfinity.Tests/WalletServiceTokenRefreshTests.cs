using System.Net;
using System.Text;
using Tripfinity.Models.Data;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Tables;
using Tripfinity.Services;
using Tripfinity.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Tripfinity.Tests;

/// <summary>
/// Verifies that WalletService reacts to WalletStation's obsolete-token
/// response code ("01") by authenticating again, persisting the fresh token
/// in the ExternalApiTokens table, and retrying the failed call once.
/// </summary>
public class WalletServiceTokenRefreshTests
{
    private sealed class ScriptedWalletHandler : HttpMessageHandler
    {
        public int AuthCalls { get; private set; }
        public int DebitCalls { get; private set; }
        public int NameEnquiryCalls { get; private set; }

        /// <summary>Number of initial responses that report the obsolete-token code "01".</summary>
        public int ObsoleteResponsesBeforeSuccess { get; set; }

        /// <summary>Response code used once the obsolete responses are exhausted.</summary>
        public string DebitResponseCode { get; set; } = "00";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath.Trim('/');

            switch (path)
            {
                case "Auth":
                    AuthCalls++;
                    return Respond(new
                    {
                        responseHeader = new { responseCode = "00", responseMessage = "ok" },
                        token = $"token-{AuthCalls}",
                        expiryDate = DateTime.Now.AddMinutes(30).ToString("O")
                    });

                case "Debit":
                {
                    DebitCalls++;
                    var code = DebitCalls <= ObsoleteResponsesBeforeSuccess ? "01" : DebitResponseCode;
                    var message = code switch
                    {
                        "01" => "Obsolete token",
                        "00" => "ok",
                        _ => "Business failure"
                    };

                    return Respond(new
                    {
                        responseHeader = new { responseCode = code, responseMessage = message },
                        amount = 100,
                        balance = 900,
                        description = "Debit",
                        transactionId = "TXN-1",
                        traceId = "TRC-1"
                    });
                }

                case "NameEnquiry":
                {
                    NameEnquiryCalls++;
                    if (NameEnquiryCalls <= ObsoleteResponsesBeforeSuccess)
                    {
                        return Respond(new
                        {
                            responseHeader = new { responseCode = "01", responseMessage = "Obsolete token" }
                        });
                    }

                    return Respond(new
                    {
                        responseHeader = new { responseCode = "00", responseMessage = "ok" },
                        accountDetails = new
                        {
                            customerId = "cust-1",
                            customerAlias = "alias",
                            bvn = "",
                            virtualAccounts = new[]
                            {
                                new
                                {
                                    accountNumber = "0123456789",
                                    bankName = "Test Bank",
                                    bankCode = "000014",
                                    firstName = "Test",
                                    lastName = "User"
                                }
                            }
                        }
                    });
                }

                default:
                    return Respond(new { }, HttpStatusCode.NotFound);
            }
        }

        private static Task<HttpResponseMessage> Respond(object payload,
            HttpStatusCode status = HttpStatusCode.OK) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8,
                    "application/json")
            });
    }

    private sealed class WalletHarness : IDisposable
    {
        private readonly IDataProtectionProvider _dataProtection;

        public ScriptedWalletHandler Handler { get; } = new();
        public TestDatabase Database { get; } = new();
        public ExternalTokenStore TokenStore { get; }
        public HttpClient Client { get; }
        public WalletService Service { get; }

        public WalletHarness()
        {
            _dataProtection = DataProtectionProvider.Create(
                new DirectoryInfo(Path.Combine(Path.GetTempPath(), "tripfinity-tests")));

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["WalletStation:Username"] = "wallet-user",
                    ["WalletStation:Password"] = "wallet-pass"
                })
                .Build();

            TokenStore = new ExternalTokenStore(
                Database.Context,
                new MemoryCache(new MemoryCacheOptions()),
                NullLogger<ExternalTokenStore>.Instance,
                _dataProtection);

            Client = new HttpClient(Handler) { BaseAddress = new Uri("https://walletstation.test/") };

            Service = new WalletService(Client, NullLogger<WalletService>.Instance, config,
                Database.Context, TokenStore);
        }

        public async Task<ExternalApiToken> GetStoredTokenAsync() =>
            await Database.Context.ExternalApiTokens
                .SingleAsync(t => t.Provider == ExternalTokenStore.Providers.WalletStation);

        public string Unprotect(string protectedValue) =>
            _dataProtection.CreateProtector("Tripfinity.ExternalApiTokens.v1").Unprotect(protectedValue);

        public void Dispose()
        {
            Client.Dispose();
            Database.Dispose();
        }
    }

    private static DebitWalletRequest DebitRequest() => new()
    {
        Amount = 100,
        CustomerId = "cust-1",
        Description = "test debit",
        TraceId = "TRC-1"
    };

    [Fact]
    public async Task Obsolete_token_response_triggers_fresh_auth_retry_and_persists_new_token()
    {
        using var harness = new WalletHarness();
        harness.Handler.ObsoleteResponsesBeforeSuccess = 1;

        var result = await harness.Service.DebitWalletAsync(DebitRequest());

        Assert.Equal("00", result.ResponseHeader.ResponseCode);
        Assert.Equal(2, harness.Handler.AuthCalls);   // initial token + fresh token
        Assert.Equal(2, harness.Handler.DebitCalls);  // failed attempt + retry

        // The fresh token is stored in the external token store table.
        var stored = await harness.GetStoredTokenAsync();
        Assert.True(stored.ExpiryDate > DateTime.Now);
        Assert.Equal("token-2", harness.Unprotect(stored.Token));
    }

    [Fact]
    public async Task Business_failure_is_returned_without_refresh_or_retry()
    {
        using var harness = new WalletHarness();
        harness.Handler.ObsoleteResponsesBeforeSuccess = 0;
        harness.Handler.DebitResponseCode = "96";

        var result = await harness.Service.DebitWalletAsync(DebitRequest());

        Assert.Equal("96", result.ResponseHeader.ResponseCode);
        Assert.Equal(1, harness.Handler.AuthCalls);
        Assert.Equal(1, harness.Handler.DebitCalls);
    }

    [Fact]
    public async Task Successful_calls_reuse_the_cached_token_without_new_auth()
    {
        using var harness = new WalletHarness();
        harness.Handler.ObsoleteResponsesBeforeSuccess = 0;

        var first = await harness.Service.DebitWalletAsync(DebitRequest());
        var second = await harness.Service.DebitWalletAsync(DebitRequest());

        Assert.Equal("00", first.ResponseHeader.ResponseCode);
        Assert.Equal("00", second.ResponseHeader.ResponseCode);
        Assert.Equal(1, harness.Handler.AuthCalls);
        Assert.Equal(2, harness.Handler.DebitCalls);
    }

    [Fact]
    public async Task Name_enquiry_obsolete_token_refreshes_and_retries_once()
    {
        using var harness = new WalletHarness();
        harness.Handler.ObsoleteResponsesBeforeSuccess = 1;

        var result = await harness.Service.WalletNameEnquiryAsync(new WalletNameEnquiryRequest
        {
            CustomerId = "cust-1"
        });

        Assert.Equal("00", result.ResponseHeader.ResponseCode);
        Assert.Equal("0123456789", result.AccountNumber);
        Assert.Equal(2, harness.Handler.AuthCalls);
        Assert.Equal(2, harness.Handler.NameEnquiryCalls);
    }
}
