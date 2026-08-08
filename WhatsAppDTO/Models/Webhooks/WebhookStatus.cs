using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookStatus
{
    public string Id { get; set; } = string.Empty;

    public WebhookStatusType Status { get; set; }

    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("recipient_id")]
    public string RecipientId { get; set; } = string.Empty;

    public WebhookConversation? Conversation { get; set; }

    public WebhookPricing? Pricing { get; set; }

    public IList<WebhookError>? Errors { get; set; }
}

public sealed class WebhookConversation
{
    public string Id { get; set; } = string.Empty;

    public WebhookConversationOrigin? Origin { get; set; }

    [JsonPropertyName("expiration_timestamp")]
    public string? ExpirationTimestamp { get; set; }
}

public sealed class WebhookConversationOrigin
{
    public WebhookConversationOriginType Type { get; set; }
}

public sealed class WebhookPricing
{
    public bool Billable { get; set; }

    [JsonPropertyName("pricing_model")]
    public string PricingModel { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}
