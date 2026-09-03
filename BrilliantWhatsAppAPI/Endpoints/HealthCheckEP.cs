using FastEndpoints;

namespace BrilliantWhatsAppAPI.Endpoints;

/// <summary>
/// Health check endpoint for clients to verify connectivity to the API.
/// </summary>
public class HealthCheckEP : EndpointWithoutRequest<HealthCheckResponse>
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Check API connectivity";
            s.Description = "Returns a simple response to verify the client is connected to the endpoint.";
        });
    }

    public override async Task<HealthCheckResponse> ExecuteAsync(CancellationToken ct)
    {
        return new HealthCheckResponse
        {
            Status = "Connected",
            Timestamp = DateTime.UtcNow,
            Message = "API is running and accessible"
        };
    }
}

/// <summary>
/// Response DTO for health check endpoint.
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// Connection status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the health check.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Additional message describing the connection status.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
