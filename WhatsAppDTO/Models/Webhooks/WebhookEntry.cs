namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookEntry
{
    public string Id { get; set; } = string.Empty;

    public IList<WebhookChange> Changes { get; set; } = new List<WebhookChange>();
}
