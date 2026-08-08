using CommonData.Common;
using CommonData.DAO;
using CommonData.VO;
using Microsoft.AspNetCore.Http;

namespace CommonData.Managers;

public class TenantManager
{
    //private readonly IList<TenantVO> _tenantList = new List<TenantVO>();
    private static IDictionary<string, TenantVO> _tenantList { get; set; } = new Dictionary<string, TenantVO>();
    private readonly TenantDAO _dao = new();


    private static IHttpContextAccessor _httpContext = new HttpContextAccessor();

    public static bool CheckDictionaryKey(string token)
    {
        return _tenantList.ContainsKey(token);
    }
    public static Guid PublicKey => Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static bool IskeyExist => _httpContext.HttpContext.Items.ContainsKey(NameKeys.TenantKey);

    public static string? CurrentKey
    {
        get
        {
            if (_httpContext.HttpContext is null || _httpContext.HttpContext.Items.Count == 0)
                return null;
            return _httpContext.HttpContext.Items[NameKeys.TenantKey] as string ?? throw new Exception("Current Tenant Key Can't Be Found");
        }
    }

    public static TenantVO? CurrentTenant
    {
        get
        {
            if (CurrentKey is string currentKey)
                return _tenantList[currentKey];
            return null;
        }
    }

    //public static TenantVO? CurrentTenant
    //{
    //    get
    //    {
    //        if (CurrentKey is string currentKey)
    //            return _tenantList[currentKey].CurrentOnlineUser;
    //        return null;
    //    }
    //    set
    //    {
    //        _tenantList[CurrentKey].CurrentOnlineUser = value;
    //    }
    //}

    /// <summary>
    /// Gets a tenant by ID. If not found in memory, fetches from DB.
    /// </summary>
    public static async Task<TenantVO> GetTenantByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty." , nameof(token));

        // Search in cached list
        var tenant = _tenantList.FirstOrDefault(t => t.Token == token);
        if (tenant != null)
            return tenant;

        // Fallback: fetch from DB
        tenant = await _dao.GetByToken(token);
        if (tenant != null)
            _tenantList.Add(tenant);

        return tenant;
    }


    /// <summary>
    /// Returns all tenants currently cached in memory.
    /// </summary>
    public IReadOnlyList<TenantVO> GetAllCachedTenants() => _tenantList.Values.ToList().AsReadOnly();

    /// <summary>
    /// Refreshes tenant list from DB.
    /// </summary>
    //public void RefreshTenants()
    //{
    //    _tenantList.Clear();
    //    _tenantList.AddRange(_tenantRepository.FetchAllTenants());
    //}
}

