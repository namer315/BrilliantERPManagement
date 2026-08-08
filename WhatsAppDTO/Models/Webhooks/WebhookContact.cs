using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookContact
{
    public WebhookProfile? Profile { get; set; }

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}
