using CommonData.Session;
using CommonData.VO;
using System.Collections.Concurrent;

namespace CommonData.Services;

/// <summary>In-memory tenant cache keyed by API token, with DB fallback on miss.</summary>
public class TenantCacheService
{
    private readonly ConcurrentDictionary<string, TenantVO> _byToken = new(StringComparer.Ordinal);

    /// <summary>The cached tenants.</summary>
    public IReadOnlyList<TenantVO> Tenants => _byToken.Values.ToList();

    /// <summary>
    /// Gets the tenant for a token. Checks the cache first; on a miss it loads
    /// from the DB, caches it, then returns. Returns null if unknown/inactive.
    /// </summary>
    public TenantVO? ResolveByToken(string token)
    {
        if (_byToken.TryGetValue(token, out var cached))
            return cached.Active ? cached : null;

        var tenant = LoadFromDb(token);
        if (tenant is not null)
            _byToken[token] = tenant;

        return tenant is { Active: true } ? tenant : null;
    }

    /// <summary>Warm the cache with all active tenants at startup.</summary>
    public void Warmup()
    {
        using var session = SessionFactoryGenerator.SessionFactory.OpenSession();
        foreach (var t in session.QueryOver<TenantVO>().Where(t => t.Active).List())
            _byToken[t.Token] = t;
    }

    private static TenantVO? LoadFromDb(string token)
    {
        using var session = SessionFactoryGenerator.SessionFactory.OpenSession();
        return session.QueryOver<TenantVO>()
            .Where(t => t.Token == token)
            .SingleOrDefault();
    }
}
