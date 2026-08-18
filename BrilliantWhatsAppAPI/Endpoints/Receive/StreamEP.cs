using FastEndpoints;
using WhatsAppFDM;

namespace BrilliantWhatsAppAPI.Endpoints.Receive;

public class StreamEP : EndpointWithoutRequest
{
    private readonly WebhookFDM _fdm = new WebhookFDM();
    public override void Configure()
    {
        Get("/Receive/Stream");
        AllowAnonymous();
        Options(x => x.RequireCors(p => p.AllowAnyOrigin()));
        //Options(x => x.WithTags("Realtime").RequireCors("AllowAll"));

    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.EventStreamAsync("stream" , _fdm.Stream(ct) , ct);
    }
}