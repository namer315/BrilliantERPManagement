namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookChange
{
    public WebhookValue Value { get; set; } = new();

    public string Field { get; set; } = string.Empty;
}
