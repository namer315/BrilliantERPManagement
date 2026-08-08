using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public sealed class WhatsAppWebhookRequest
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    public IList<WebhookEntry> Entry { get; set; } = new List<WebhookEntry>();
}
