using FastEndpoints;
using System.Text.Json;
using WhatsAppData.DTO.Webhooks;
using WhatsAppDTO.Management.Webhooks;
using WhatsAppDTO.Models.Webhooks;
using WhatsAppFDM;

namespace BrilliantWhatsAppAPI.Endpoints;

public class WebhookEP : Endpoint</*WhatsAppWebhookRequest*/  WebhookDTO , WebhookResponse>
{
    private WebhooksManager _webhooksManager = new WebhooksManager();
    private WebhookFDM _fdm = new WebhookFDM();
    private readonly IConfiguration _configuration;

    public WebhookEP(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void Configure()
    {
        Post("/webhook/whatsapp");
        AllowAnonymous();
    }

    public override async Task<WebhookResponse> ExecuteAsync(WebhookDTO req , CancellationToken ct)
    {
        try
        {
            //Console.WriteLine($"Webhook received: {req.Object}");

            // Route each entry/change by type
            //foreach (var entry in req.Entry)
            //{
            //    foreach (var change in entry.Changes)
            //    {
            //        ProcessMessages(change.Value);
            //        ProcessStatuses(change.Value);
            //    }
            //}

            //bool shouldSaveRequestToFile = await _webhooksManager.HandleWebhookRequest(req);
            bool writeToFile = _configuration.GetValue<bool>("Webhook:WriteToFile");
            bool shouldSaveRequestToFile = await _fdm.HandleWebhook(req);

            if (writeToFile || shouldSaveRequestToFile)
            {
                WriteRequestInFile();
            }
        }
        catch (Exception ex)
        {

            WriteRequestInFile();
        }

        return new WebhookResponse { Status = "processed" };
    }

    private async Task WriteRequestInFile()
    {
        try
        {
            var httpContext = HttpContext;

            // Read raw body
            httpContext.Request.Body.Position = 0;
            using var reader = new StreamReader(httpContext.Request.Body , leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            httpContext.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(rawBody))
            {
                // Parse and re‑serialize with indentation
                using var doc = JsonDocument.Parse(rawBody);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var prettyJson = JsonSerializer.Serialize(doc.RootElement , options);

                // Generate timestamped filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");

                // Ensure folder exists
                var webhookFolder = Path.Combine(AppContext.BaseDirectory , "webhooks");
                Directory.CreateDirectory(webhookFolder);

                // Build file path
                var filePath = Path.Combine(webhookFolder , $"{timestamp}.json");

                await File.WriteAllTextAsync(filePath , prettyJson);
            }
        }
        catch (Exception ex)
        {

        }
    }

    private static void ProcessMessages(WebhookValue value)
    {
        if (value.Messages is not { Count: > 0 })
            return;

        foreach (var msg in value.Messages)
        {
            Console.WriteLine($"  [{msg.Type}] from={msg.From} id={msg.Id}");

            switch (msg.Type)
            {
                case WebhookMessageType.Text when msg.Text is not null:
                    Console.WriteLine($"    body={msg.Text.Body}");
                    break;
                case WebhookMessageType.Image when msg.Image is not null:
                    Console.WriteLine($"    image_id={msg.Image.Id}");
                    break;
                case WebhookMessageType.Interactive when msg.Interactive is not null:
                    Console.WriteLine($"    interactive_type={msg.Interactive.Type}");
                    break;
                case WebhookMessageType.Location when msg.Location is not null:
                    Console.WriteLine($"    lat={msg.Location.Latitude} lon={msg.Location.Longitude}");
                    break;
            }
        }
    }

    private static void ProcessStatuses(WebhookValue value)
    {
        if (value.Statuses is not { Count: > 0 })
            return;

        foreach (var s in value.Statuses)
        {
            Console.WriteLine($"  [status] id={s.Id} status={s.Status} recipient={s.RecipientId}");

            if (s.Errors is { Count: > 0 })
            {
                foreach (var err in s.Errors)
                    Console.WriteLine($"    error {err.Code}: {err.Title}");
            }
        }
    }
}

public sealed class WebhookResponse
{
    public string Status { get; set; } = string.Empty;
}

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

        // var expectedToken = _configuration["WhatsApp:WebhookVerifyToken"];
        var expectedToken = _configuration["Webhook:VerifyToken"];

        if (string.Equals(mode , "subscribe" , StringComparison.Ordinal)
            && !string.IsNullOrEmpty(challenge)
            && string.Equals(token , expectedToken , StringComparison.Ordinal))
        {
            // Echo the challenge back as plain text with HTTP 200.
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsync(challenge , ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}

