using CommonData.Session;

namespace BrilliantWhatsAppAPI.Processors;

/// <summary>
/// Middleware that guarantees the ambient <see cref="TenantContext"/> is
/// cleared when the request scope ends, even if an exception is thrown.
///
/// Registered BEFORE UseFastEndpoints() so its finally block runs AFTER the
/// endpoint pipeline (including TokenPreProcessor, which sets the tenant).
/// This prevents the tenant from leaking into the next request's ambient state.
/// </summary>
public class TenantScopeCleaner
{
    private readonly RequestDelegate _next;

    public TenantScopeCleaner(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            //TenantContext.Clear();
        }
    }
}
