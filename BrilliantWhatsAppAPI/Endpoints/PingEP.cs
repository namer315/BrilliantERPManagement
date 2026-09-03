using FastEndpoints;

namespace BrilliantWhatsAppAPI.Endpoints;

public class PingEP : EndpointWithoutRequest<PingResponseDTO>
{
    public override void Configure()
    {
        Get("/Ping");
        AllowAnonymous();
    }

    public override async Task<PingResponseDTO> ExecuteAsync(CancellationToken ct)
        => new PingResponseDTO { Status = "Connected" , ServerTimeUtc = DateTime.UtcNow };
}

public class PingResponseDTO
{
    public string Status { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}