using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Tripfinity.Utilities;

/// <summary>
/// Builds and verifies the value embedded in a passenger's QR code.
/// The reference is encrypted and authenticated with Data Protection, so a
/// payload cannot be forged or tampered with even if the reference format is
/// known, and an attacker cannot enumerate ticket references to produce a
/// scannable code.
/// </summary>
public sealed class TicketQrPayload
{
    private const string Prefix = "TKT1.";
    private readonly IDataProtector _protector;

    public TicketQrPayload(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("Tripfinity.TicketQr.v1");
    }

    public string Protect(string ticketReference) =>
        Prefix + Base64UrlEncode(_protector.Protect(Encoding.UTF8.GetBytes(ticketReference)));

    /// <summary>
    /// Returns the ticket reference only when <paramref name="payload"/> was
    /// produced by <see cref="Protect"/>. Tampered or unsigned values fail.
    /// </summary>
    public bool TryUnprotect(string? payload, out string ticketReference)
    {
        ticketReference = string.Empty;

        if (string.IsNullOrWhiteSpace(payload)) return false;

        var trimmed = payload.Trim();
        if (!trimmed.StartsWith(Prefix, StringComparison.Ordinal)) return false;

        var encoded = trimmed[Prefix.Length..];
        if (encoded.Length == 0) return false;

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(_protector.Unprotect(Base64UrlDecode(encoded)));
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(decoded)) return false;

        ticketReference = decoded;
        return true;
    }

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = (padded.Length % 4) switch
        {
            2 => padded + "==",
            3 => padded + "=",
            _ => padded
        };
        return Convert.FromBase64String(padded);
    }
}
