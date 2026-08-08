using CommonData.Services;
using CommonData.Session;
using CommonData.VO;
using CommonFDM;
using FastEndpoints;

namespace BrilliantWhatsAppAPI.Processors;

public class TokenPreProcessor : IGlobalPreProcessor
{
    TenantFDM _fdm = new TenantFDM();
    public TokenPreProcessor(IConfiguration configuration)
    {
        // IConfiguration is a singleton — safe for constructor injection.
        // Scoped dependencies (ITenantContextAccessor) are resolved
        // from HttpContext.RequestServices at runtime.
    }

    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        // The WhatsApp/Meta webhook verification handshake is an unauthenticated
        // GET that carries hub.mode / hub.verify_token / hub.challenge as query
        // parameters — there is NO Authorization header. Global pre-processors run
        // on every endpoint regardless of AllowAnonymous(), so this request must be
        // explicitly exempted from bearer-token enforcement here.
        if (IsWebhookVerifyRequest(context.HttpContext))
        {
            //await Task.CompletedTask;
            return;
        }

        if (TryExtractToken(context, out var token))
        {
            // Resolve the tenant from the in-memory cache, falling back to the
            // DB on a cache miss (TenantCacheService). Returns the full TenantVO
            // including Id so the DAL tenant filter receives a real tenant id.
            var cache = context.HttpContext.RequestServices
                .GetRequiredService<TenantCacheService>();

            
            if (await _fdm.ResolveTenantByToken(token) is TenantVO tenantVO)
            {
                // Store in HttpContext.Items (backward compat) and the ambient
                // TenantContext so it is readable from any method in the solution.
                context.HttpContext.Items["Tenant"] = tenantVO;
                TenantContext.CurrentTenant = tenantVO; 
            }
            else
            {
                throw new UnauthorizedAccessException(
                    "Invalid API token: the provided token does not match any registered tenant, or the tenant is deactivated.");
            }

            //if (cache.ResolveByToken(token) is TenantVO tenantVO)
            //{
            //    // Store in HttpContext.Items (backward compat) and the ambient
            //    // TenantContext so it is readable from any method in the solution.
            //    context.HttpContext.Items["Tenant"] = tenantVO;
            //    TenantContext.CurrentTenant = tenantVO;
            //}
            //else
            //{
            //    throw new UnauthorizedAccessException(
            //        "Invalid API token: the provided token does not match any registered tenant, or the tenant is deactivated.");
            //}
        }
        else
        {            
            throw new UnauthorizedAccessException("Authentication required. The request must include a valid Bearer token in the Authorization header.");
        }
    }

    /// <summary>
    /// Determines whether the current request is the WhatsApp/Meta webhook
    /// verification GET handshake (hub.mode / hub.verify_token / hub.challenge).
    /// These unauthenticated GETs must bypass bearer-token enforcement.
    /// </summary>
    private static bool IsWebhookVerifyRequest(HttpContext httpContext)
    {
        //if (!HttpMethods.IsGet(httpContext.Request.Method))
        //    return false;

        var path = httpContext.Request.Path.Value ?? string.Empty;
        return path.EndsWith("/webhook/whatsapp", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts the bearer token from the Authorization header of the incoming HTTP request.
    /// Supports both "Bearer &lt;token&gt;" and raw token formats.
    /// </summary>
    /// <param name="context">The global pre-processor context containing the HTTP request.</param>
    /// <param name="token">When this method returns <c>true</c>, contains the extracted token string; otherwise, <c>null</c> or empty.</param>
    /// <returns><c>true</c> if a non-empty token was successfully extracted; otherwise, <c>false</c>.</returns>
    private static bool TryExtractToken(IPreProcessorContext context, out string token)
    {
        token = string.Empty;

        if (!context.HttpContext.Request.Headers.TryGetValue(
                "Authorization", out var header))
            return false;

        var value = header.ToString();

        if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            token = value["Bearer ".Length..];
        else
            token = value; // tolerate raw key for flexibility

        return !string.IsNullOrWhiteSpace(token);
    }
}