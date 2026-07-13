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

    public async Task PreProcessAsync(IPreProcessorContext ctx, CancellationToken ct)
    {
        if (TryExtractToken(ctx, out var token) /*|| token != _apiKey*/)
        {
            if(!TokenService.ValidateToken(token))
            {
                await WriteUnauthorized(ctx, ct, "Invalid or missing API key");
            }
            else
            {

            }
        }
        else
        {
            await WriteUnauthorized(ctx , ct , "token not exist");
        }
    }

    // --------------------------------------------------------
    // Extracts the Bearer token, or falls back to raw header.
    // --------------------------------------------------------
    private static bool TryExtractToken(IPreProcessorContext ctx, out string token)
    {
        token = string.Empty;

        if (!ctx.HttpContext.Request.Headers.TryGetValue(
                "Authorization", out var header))
            return false;

        var value = header.ToString();

        if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            token = value["Bearer ".Length..];
        else
            token = value; // tolerate raw key for flexibility

        return !string.IsNullOrWhiteSpace(token);
    }

    // --------------------------------------------------------
    // Writes a JSON 401 and halts the pipeline.
    // --------------------------------------------------------
    private static async Task WriteUnauthorized(
        IPreProcessorContext ctx, CancellationToken ct, string message)
    {
        var response = ctx.HttpContext.Response;

        response.StatusCode  = StatusCodes.Status401Unauthorized;
        response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { error = message });

        await response.WriteAsync(body, ct);
    }
}