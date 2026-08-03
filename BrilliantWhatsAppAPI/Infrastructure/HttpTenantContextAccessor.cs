using CommonData.Session;

namespace BrilliantWhatsAppAPI.Infrastructure;

/// <summary>
/// HTTP-scoped implementation of ITenantContextAccessor.
/// Tenant is set by TokenPreProcessor during authentication and read by the DAL.
/// </summary>
public class HttpTenantContextAccessor : ITenantContextAccessor
{
    /// <summary>
    /// The current tenant for this HTTP request. Set during token validation.
    /// </summary>
    public CommonData.VO.TenantVO? CurrentTenant { get; set; }
}
