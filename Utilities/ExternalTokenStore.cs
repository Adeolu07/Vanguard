using System.Security.Cryptography;
using Tripfinity.Models.Data;
using Tripfinity.Models.Tables;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Tripfinity.Utilities;

public class ExternalTokenStore
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExternalTokenStore> _logger;
    private readonly IDataProtector _protector;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    public record ExternalApiCredential(string Token, string Key, DateTime ExpiryDate);

    public ExternalTokenStore(AppDbContext context, IMemoryCache cache, ILogger<ExternalTokenStore> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _protector = dataProtectionProvider.CreateProtector("Tripfinity.ExternalApiTokens.v1");
    }

    // Partner bearer tokens and signing keys must never be readable straight out
    // of a database backup, so they are encrypted at rest with Data Protection.
    private string Protect(string value) => string.IsNullOrEmpty(value) ? value : _protector.Protect(value);

    private string Unprotect(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        try
        {
            return _protector.Unprotect(value);
        }
        catch (CryptographicException)
        {
            // Row written before encryption was introduced.
            return value;
        }
    }

    /// <summary>
    /// Returns the stored token for a provider (or acquires one through
    /// <paramref name="acquireFreshToken"/> if none is unexpired).
    /// Flow per request: memory cache -> database -> provider auth endpoint.
    /// When <paramref name="forceRefresh"/> is true, cached and stored tokens
    /// are ignored and a fresh token is always acquired and persisted.
    /// </summary>
    public async Task<string> GetTokenAsync(
        string provider,
        Func<Task<(string Token, DateTime ExpiryDate)>> acquireFreshToken,
        bool forceRefresh = false)
    {
        var cacheKey = $"Token:{provider}";

        if (!forceRefresh && _cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            _logger.LogDebug("{Provider} token from memory cache", provider);
            return cached;
        }

        if (!forceRefresh)
        {
            var stored = await _context.ExternalApiTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Provider == provider && t.ExpiryDate > DateTime.Now);

            if (stored is not null)
            {
                var plaintext = Unprotect(stored.Token);
                _cache.Set(cacheKey, plaintext, stored.ExpiryDate - DateTime.Now);
                _logger.LogDebug("{Provider} token from database", provider);
                return plaintext;
            }
        }

        await RefreshLock.WaitAsync();
        try
        {
            // Re-check after acquiring the lock (another request may have just refreshed).
            if (!forceRefresh && _cache.TryGetValue(cacheKey, out string? cachedAfterLock) && cachedAfterLock is not null)
            {
                _logger.LogDebug("{Provider} token from memory cache (after lock)", provider);
                return cachedAfterLock;
            }

            var (freshToken, expiresAt) = await acquireFreshToken();

            var existing = await _context.ExternalApiTokens.FirstOrDefaultAsync(t => t.Provider == provider);
            if (existing is null)
            {
                _context.ExternalApiTokens.Add(new ExternalApiToken
                {
                    Provider = provider,
                    Token = Protect(freshToken),
                    ExpiryDate = expiresAt,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                existing.Token = Protect(freshToken);
                existing.ExpiryDate = expiresAt;
                existing.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            _cache.Set(cacheKey, freshToken, expiresAt - DateTime.Now);
            _logger.LogInformation("Acquired fresh {Provider} token", provider);

            return freshToken;
        }
        finally
        {
            RefreshLock.Release();
        }
    }
    
     public async Task<ExternalApiCredential> GetCredentialAsync(
            string provider,
            Func<Task<ExternalApiCredential>> acquireFreshCredential,
            bool forceRefresh = false)
        {
            var cacheKey = $"Credential:{provider}";

            if (!forceRefresh && _cache.TryGetValue(cacheKey, out ExternalApiCredential? cached) && cached is not null)
            {
                _logger.LogDebug("{Provider} credential from memory cache", provider);
                return cached;
            }

            if (!forceRefresh)
            {
                var stored = await _context.ExternalApiTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Provider == provider && t.ExpiryDate > DateTime.Now);

                if (stored is not null)
                {
                    var fromDb = new ExternalApiCredential(
                        Unprotect(stored.Token),
                        Unprotect(stored.Key ?? string.Empty),
                        stored.ExpiryDate);
                    _cache.Set(cacheKey, fromDb, stored.ExpiryDate - DateTime.Now);
                    _logger.LogDebug("{Provider} credential from database", provider);
                    return fromDb;
                }
            }

            await RefreshLock.WaitAsync();
            try
            {
                // Re-check after acquiring the lock (another request may have just refreshed).
                if (!forceRefresh && _cache.TryGetValue(cacheKey, out ExternalApiCredential? cachedAfterLock) && cachedAfterLock is not null)
                {
                    _logger.LogDebug("{Provider} credential from memory cache (after lock)", provider);
                    return cachedAfterLock;
                }

                var fresh = await acquireFreshCredential();

                var existing = await _context.ExternalApiTokens.FirstOrDefaultAsync(t => t.Provider == provider);
                if (existing is null)
                {
                    _context.ExternalApiTokens.Add(new ExternalApiToken
                    {
                        Provider = provider,
                        Token = Protect(fresh.Token),
                        Key = Protect(fresh.Key),
                        ExpiryDate = fresh.ExpiryDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                else
                {
                    existing.Token = Protect(fresh.Token);
                    existing.Key = Protect(fresh.Key);
                    existing.ExpiryDate = fresh.ExpiryDate;
                    existing.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _cache.Set(cacheKey, fresh, fresh.ExpiryDate - DateTime.Now);
                _logger.LogInformation("Acquired fresh {Provider} credential", provider);

                return fresh;
            }
            finally
            {
                RefreshLock.Release();
            }
        }

    public static class Providers
    {
        public const string FastChannel = "FastChannel";
        public const string WalletStation = "WalletStation";
    }
}