using Tripfinity.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Tripfinity.Tests;

public class PublicUrlTests
{
    private static IConfiguration Config(string? baseUrl) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["App:PublicBaseUrl"] = baseUrl })
            .Build();

    [Fact]
    public void Uses_configured_base_url_instead_of_the_request_host()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("evil.example");

        var url = PublicUrl.Build(context, Config("https://app.tripfinity.test"), "/Auth/ConfirmEmail?userId=1&token=abc");

        Assert.Equal("https://app.tripfinity.test/Auth/ConfirmEmail?userId=1&token=abc", url);
    }

    [Fact]
    public void Falls_back_to_the_request_when_no_base_url_is_configured()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("localhost:7160");

        var url = PublicUrl.Build(context, Config(null), "/Auth/ResetPassword?token=x");

        Assert.Equal("https://localhost:7160/Auth/ResetPassword?token=x", url);
    }

    [Fact]
    public void Ignores_a_relative_public_base_url()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("real-host");

        var url = PublicUrl.Build(context, Config("/not-absolute"), "/x");

        Assert.Equal("https://real-host/x", url);
    }
}
