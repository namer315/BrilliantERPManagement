using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookValue
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    public WebhookMetadata Metadata { get; set; } = new();

    public IList<WebhookContact>? Contacts { get; set; }

    public IList<WebhookInboundMessage>? Messages { get; set; }

    public IList<WebhookStatus>? Statuses { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
