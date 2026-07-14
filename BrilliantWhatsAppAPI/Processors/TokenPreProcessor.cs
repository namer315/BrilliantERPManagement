using System.Text.Json;
using FastEndpoints;
using BrilliantWhatsAppAPI.Services;

namespace BrilliantWhatsAppAPI.Processors;

public class TokenPreProcessor : IGlobalPreProcessor
{
    //private readonly string _apiKey;

    public TokenPreProcessor(IConfiguration configuration)
    {
        //_apiKey = configuration["Authentication:ApiKey"]
        //    ?? throw new InvalidOperationException(
        //        "Authentication:ApiKey is not configured.");
    }

    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        if (TryExtractToken(context, out var token) /*|| token != _apiKey*/)
        {
            if(TokenService.ValidateToken(token) is Tenant tenant)
            {
                // Store the authenticated tenant in HttpContext.Items for downstream endpoints
                context.HttpContext.Items["Tenant"] = tenant;
            }
        }
        else
        {            
            throw new UnauthorizedAccessException("Authentication required. The request must include a valid Bearer token in the Authorization header.");
        }
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