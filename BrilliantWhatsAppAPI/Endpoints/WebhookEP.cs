using FastEndpoints;
using System.Text.Json;
using WhatsAppData.DTO.Webhooks;
using WhatsAppFDM;

namespace BrilliantWhatsAppAPI.Endpoints;

public class WebhookEP : Endpoint<WebhookDTO , WebhookResponse>
{
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
            bool writeToFile = _configuration.GetValue<bool>("Webhook:WriteToFile");
            bool shouldSaveRequestToFile = await _fdm.HandleWebhook(req);

            if (writeToFile || shouldSaveRequestToFile)
            {
                WriteRequestInFile();
            }
        }
        catch (Exception ex)
        {
            WriteRequestInFile("_ex");
            throw ex;
        }

        return new WebhookResponse { Status = "processed" };
    }

    private async Task WriteRequestInFile(string suffix = "")
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

