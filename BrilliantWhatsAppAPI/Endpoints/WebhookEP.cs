using FastEndpoints;
using Microsoft.AspNetCore.Http;
using FluentNHibernate.Conventions.Helpers;
using System.Text.Json;

namespace BrilliantWhatsAppAPI.Endpoints;

public class WebhookEP : Endpoint<WhatsAppWebhookRequest , WhatsAppWebhookResponse>
{
    public override void Configure()
    {
        Post("/webhook/whatsapp");   // WhatsApp will POST here
        AllowAnonymous();            // must be public for WhatsApp
    }

    public override async Task<WhatsAppWebhookResponse> ExecuteAsync(WhatsAppWebhookRequest req , CancellationToken ct)
    {
        // Log or process the webhook payload
        Console.WriteLine($"Webhook received: {req.Object}");

        // var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        // var filePath = Path.Combine("wwwroot" , "webhooks" , $"{timestamp}.json");
        // await File.WriteAllTextAsync(filePath , JsonSerializer.Serialize(req.Entry));

        /*var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        var webhookFolder = Path.Combine(AppContext.BaseDirectory, "webhooks");
        Directory.CreateDirectory(webhookFolder); // ensure the folder exists
        var filePath = Path.Combine(webhookFolder , $"{timestamp}.json");
        await File.WriteAllTextAsync(filePath , JsonSerializer.Serialize(req));*/

        // Generate timestamped filename
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");

        // Ensure folder exists
        var webhookFolder = Path.Combine(AppContext.BaseDirectory , "webhooks");
        Directory.CreateDirectory(webhookFolder);

        // Build file path
        var filePath = Path.Combine(webhookFolder , $"{timestamp}.json");

        // Serialize with indentation for readability
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(req , options);

        // Write to file
        await File.WriteAllTextAsync(filePath , json);

        // Example: handle messages
        //if (req.Entry != null)
        //{
        //    Console.WriteLine($"Entry: {req.Entry}");
        //}

        return new WhatsAppWebhookResponse
        {
            Status = "Webhook processed"
        };
    }
}

public class WhatsAppWebhookRequest
{
    public string Object { get; set; }
    public dynamic Entry { get; set; } // flexible for nested JSON
}
public class WhatsAppWebhookResponse
{
    public string Status { get; set; }
}

// Verify that the webhook endpoint is reachable and correctly configured in your WhatsApp Business API settings.
public class WhatsAppWebhookVerifyEndpoint : EndpointWithoutRequest
{
    private readonly IConfiguration _configuration;

    public WhatsAppWebhookVerifyEndpoint(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void Configure()
    {
        Get("/webhook/whatsapp");   // same route, but GET
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var mode = Query<string>("hub.mode");
        var token = Query<string>("hub.verify_token");
        var challenge = Query<string>("hub.challenge");

        var expectedToken = _configuration["WhatsApp:WebhookVerifyToken"];

        if (string.Equals(mode, "subscribe", StringComparison.Ordinal)
            && !string.IsNullOrEmpty(challenge)
            && string.Equals(token, expectedToken, StringComparison.Ordinal))
        {
            // Echo the challenge back as plain text with HTTP 200.
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsync(challenge, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}

