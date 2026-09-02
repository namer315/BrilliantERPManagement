using CommonData.Managers;
using CommonData.Session;
using CommonData.VO;
using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.Managers;

/// <summary>
/// In-memory registry of <see cref="WhatsAppTenantVO"/> models, keyed by the
/// underlying ERP <see cref="TenantVO.Id"/> (the "Tenant" reference).
///
/// Mirrors <see cref="CommonData.Managers.TenantManager"/> but works with the
/// full WhatsApp tenant model (credentials, contact and tenant). It is lazily
/// populated by <c>GetWhatsAppTenantByTenant</c>, which fetches from the DB on
/// a cache miss and registers the result so subsequent calls are served from
/// memory.
/// </summary>
public static class WhatsAppTenantManager
{
    private static readonly IDictionary<Guid , WhatsAppTenantVO> _whatsAppTenantList = new Dictionary<Guid , WhatsAppTenantVO>();

    private static readonly WhatsAppTenantDAO _dao = new WhatsAppTenantDAO();

    /// <summary>
    /// Returns true when a <see cref="WhatsAppTenantVO"/> is registered for the
    /// given ERP tenant id.
    /// </summary>
    public static bool CheckDictionaryKey(Guid tenantId)
    {
        return _whatsAppTenantList.ContainsKey(tenantId);
    }
    public static bool IskeyExist => TenantManager.IskeyExist;
    /// <summary>
    /// The <see cref="WhatsAppTenantVO"/> registered for the current request's
    /// tenant, or <c>null</c> if none is registered.
    ///
    /// The tenant id is read from the ambient <see cref="TenantContext"/> (the
    /// AsyncLocal set by authentication), NOT from the static dictionary, so it
    /// is reliable even on code paths that bypass the Tenancy pre-processor.
    /// </summary>
    public static WhatsAppTenantVO? CurrentWhatsAppTenant
    {
        get
        {
            var tenantId = TenantManager.CurrentTenant?.Id;
            if (tenantId is not Guid id)
                return null;

            return _whatsAppTenantList.TryGetValue(id , out var value)
                ? value
                : null;
        }
    }

    public static ContactVO? CurrentContact => CurrentWhatsAppTenant?.Contact;


    /// <summary>
    /// Returns the <see cref="WhatsAppTenantVO"/> for the given ERP tenant,
    /// loading it from the DB and registering it in the cache on a miss.
    /// Returns <c>null</c> when the tenant has no WhatsApp tenant record.
    /// </summary>
    public static async Task<WhatsAppTenantVO?> GetWhatsAppTenantByTenant(TenantVO tenant)
    {
        if (tenant is null || tenant.Id == Guid.Empty)
            //if (tenant?.Id == Guid.Empty || tenant?.Id is not Guid tenantId)
            return null;

        // Cache hit
        if (_whatsAppTenantList.TryGetValue(tenant.Id , out var cached))
            return cached;

        // Cache miss: fetch the full model and register it
        var whatsAppTenant = await _dao.GetByTenantIdAsync(tenant.Id);
        if (whatsAppTenant != null)
            _whatsAppTenantList[tenant.Id] = whatsAppTenant;

        return whatsAppTenant;
    }

    /// <summary>
    /// Resolves the ERP tenant by token, then loads and registers the full
    /// <see cref="WhatsAppTenantVO"/>. The resolved tenant is read from the
    /// ambient <see cref="TenantContext"/>, set by authentication.
    /// </summary>
    public static async Task<WhatsAppTenantVO?> GetWhatsAppTenantByToken(string token)
    {
        var currentTenant = TenantManager.CurrentTenant;
        if (currentTenant is null)
            return null;

        return await GetWhatsAppTenantByTenant(currentTenant);
    }

    /// <summary>
    /// Registers (or replaces) the given <see cref="WhatsAppTenantVO"/> in the
    /// cache under its ERP tenant id.
    /// </summary>
    public static void Register(WhatsAppTenantVO whatsAppTenant)
    {
        if (whatsAppTenant?.Tenant?.Id is Guid tenantId && tenantId != Guid.Empty)
            _whatsAppTenantList[tenantId] = whatsAppTenant;
    }

    /// <summary>
    /// Returns all WhatsApp tenants currently registered in memory.
    /// </summary>
    public static IReadOnlyList<WhatsAppTenantVO> GetAllCachedWhatsAppTenants()
        => _whatsAppTenantList.Values.ToList().AsReadOnly();
}

