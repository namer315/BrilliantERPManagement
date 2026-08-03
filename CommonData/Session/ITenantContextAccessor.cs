namespace CommonData.Session;

/// <summary>
/// Abstraction to resolve the current tenant at the DAL level
/// without depending on ASP.NET Core. The web layer provides
/// an implementation via IHttpContextAccessor.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// The tenant for the current request scope, or null if unauthenticated.
    /// The web layer sets this during token validation.
    /// </summary>
    VO.TenantVO? CurrentTenant { get; set; }
}
