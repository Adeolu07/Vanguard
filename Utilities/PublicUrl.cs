namespace Tripfinity.Utilities;

/// <summary>
/// Builds absolute links for emails. Prefer the configured public base URL so a
/// spoofed <c>Host</c> header can never end up inside a password-reset or
/// confirmation link; fall back to the current request when it is not set.
/// </summary>
public static class PublicUrl
{
    public static string Build(HttpContext httpContext, IConfiguration configuration, string path)
    {
        var configured = configuration["App:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured)
            && Uri.TryCreate(configured.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri)
            && (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps))
        {
            return new Uri(baseUri, path.TrimStart('/')).ToString();
        }

        var host = httpContext.Request.Host;
        var hostValue = host.HasValue && IsValidHost(host.Value) ? host.Value : "localhost";
        return $"{httpContext.Request.Scheme}://{hostValue}{path}";
    }

    private static bool IsValidHost(string value) =>
        value.Length is > 0 and <= 253
        && !value.Any(c => c is '/' or '\\' or '@' or ' ' or '#' or '?');
}
