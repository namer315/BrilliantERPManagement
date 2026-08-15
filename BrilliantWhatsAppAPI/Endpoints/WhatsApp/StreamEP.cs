using CommonData.Managers;
using FastEndpoints;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WhatsAppFDM;

namespace BrilliantWhatsAppAPI.Endpoints.WhatsApp;

public class StreamEP : EndpointWithoutRequest
{
    private readonly WebhookFDM _fdm = new WebhookFDM();
    public override void Configure()
    {
        Get("/Received/Stream");
        AllowAnonymous();
        Options(x => x.RequireCors(p => p.AllowAnyOrigin()));
        //Options(x => x.WithTags("Realtime").RequireCors("AllowAll"));

    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.EventStreamAsync("stream" , _fdm.Stream(ct) , ct);
    }
}