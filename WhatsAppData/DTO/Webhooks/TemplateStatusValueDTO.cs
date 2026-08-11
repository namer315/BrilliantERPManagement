using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class TemplateStatusValueDTO
{
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("message_template_id")]
    public long MessageTemplateId { get; set; }

    [JsonPropertyName("message_template_name")]
    public string MessageTemplateName { get; set; } = string.Empty;

    [JsonPropertyName("message_template_language")]
    public string MessageTemplateLanguage { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("message_template_category")]
    public string MessageTemplateCategory { get; set; } = string.Empty;
}
