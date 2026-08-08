using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookInboundMessage : WebhookMessage
{
    public WebhookText? Text { get; set; }

    public WebhookMedia? Image { get; set; }

    public WebhookMedia? Video { get; set; }

    public WebhookMedia? Audio { get; set; }

    public WebhookMedia? Document { get; set; }

    public WebhookMedia? Sticker { get; set; }

    public WebhookInteractive? Interactive { get; set; }

    public WebhookLocation? Location { get; set; }

    public IList<WebhookContactItem>? Contacts { get; set; }

    public WebhookReaction? Reaction { get; set; }

    public WebhookOrder? Order { get; set; }

    public WebhookSystem? System { get; set; }

    public WebhookReferral? Referral { get; set; }
}

public sealed class WebhookReferral
{
    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("source_type")]
    public string? SourceType { get; set; }

    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    public string? Headline { get; set; }

    public string? Body { get; set; }
}
