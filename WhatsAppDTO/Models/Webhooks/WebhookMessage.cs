using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public abstract class WebhookMessage
{
    public string From { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public string Timestamp { get; set; } = string.Empty;

    public WebhookMessageType Type { get; set; }

    public WebhookContext? Context { get; set; }

    public WebhookIdentity? Identity { get; set; }
}

public sealed class WebhookContext
{
    public string From { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;
}

public sealed class WebhookIdentity
{
    public bool Acknowledged { get; set; }

    [JsonPropertyName("created_timestamp")]
    public string? CreatedTimestamp { get; set; }

    public string? Hash { get; set; }

    [JsonPropertyName("customer_identity_changed")]
    public bool CustomerIdentityChanged { get; set; }
}
