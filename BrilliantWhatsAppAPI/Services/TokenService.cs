using System.Security.Cryptography;

namespace BrilliantWhatsAppAPI.Services;

/// <summary>
/// Utility for generating tenant API tokens.
///
/// NOTE: Tenant authentication no longer reads from a JSON file. The in-memory
/// <see cref="CommonData.Services.TenantCacheService"/> (seeded from the DB) is
/// the source of truth: it checks the cache first and falls back to the DB on a
/// cache miss. This class only remains for generating new tokens when seeding
/// tenants into the database.
/// </summary>
public static class TokenService
{
    /// <summary>
    /// Generates a cryptographically random API token in the form "Brilliant-sk-&lt;base64&gt;".
    /// </summary>
    public static string GenerateApiToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return "Brilliant-sk-" + Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
    }
}
