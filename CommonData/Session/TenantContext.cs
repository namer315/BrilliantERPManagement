using CommonData.VO;

namespace CommonData.Session;

/// <summary>
/// Ambient (AsyncLocal-backed) holder for the current tenant.
///
/// Unlike the scoped <see cref="ITenantContextAccessor"/> (which must be
/// resolved through DI), this static context is readable from ANY method in
/// the solution — including static and non-DI code paths — via
/// <see cref="CurrentTenant"/>.
///
/// AsyncLocal flows the value through the async call chain and is isolated
/// per logical execution context, so concurrent requests see their own tenant.
/// The web layer sets the value during authentication and MUST clear it when
/// the request scope ends (see <c>TenantScopeCleaner</c>) to prevent leakage.
/// </summary>
public static class TenantContext
{
    private static readonly AsyncLocal<TenantVO?> _current = new();

    /// <summary>
    /// The tenant for the current async execution context, or <c>null</c> if
    /// unauthenticated or outside a request scope.
    /// </summary>
    public static TenantVO? CurrentTenant
    {
        get => _current.Value;
        set => _current.Value = value;
    }

    /// <summary>
    /// Clears the current tenant for this execution context.
    /// Call when the request scope ends to avoid cross-request leakage.
    /// </summary>
    public static void Clear()
    {
        _current.Value = null;
    }
}
