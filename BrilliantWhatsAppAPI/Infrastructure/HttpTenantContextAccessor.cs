using CommonData.Session;

namespace BrilliantWhatsAppAPI.Infrastructure;

/// <summary>
/// HTTP-scoped implementation of ITenantContextAccessor.
/// Tenant is set by TokenPreProcessor during authentication and read by the DAL.
/// The value is delegated to the ambient <see cref="TenantContext"/> so there is
/// a single source of truth, readable from any method in the solution.
/// </summary>
public class HttpTenantContextAccessor : ITenantContextAccessor
{
    /// <summary>
    /// The current tenant for this HTTP request. Set during token validation.
    /// </summary>
    public CommonData.VO.TenantVO? CurrentTenant
    {
        get => TenantContext.CurrentTenant;
        set => TenantContext.CurrentTenant = value;
    }
}
