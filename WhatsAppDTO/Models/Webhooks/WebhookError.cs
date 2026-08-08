using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookError
{
    public int Code { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    [JsonPropertyName("error_data")]
    public WebhookErrorData? ErrorData { get; set; }

    public string? Href { get; set; }
}

public sealed class WebhookErrorData
{
    public string Details { get; set; } = string.Empty;
}
