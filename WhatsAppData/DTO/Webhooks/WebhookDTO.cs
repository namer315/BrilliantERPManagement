using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.DTO.Webhooks;

/// <summary>
/// Top-level webhook request payload sent by Meta to the webhook endpoint.
/// Contains the WhatsApp Business Account object and a list of entries (each carrying webhook changes).
/// <para>
/// Related documentation:
/// <list type="bullet">
/// <item><description>Webhooks overview: https://developers.facebook.com/documentation/business-messaging/whatsapp/webhooks/overview</description></item>
/// <item><description>Create a webhook endpoint: https://developers.facebook.com/documentation/business-messaging/whatsapp/webhooks/create-webhook-endpoint</description></item>
/// <item><description>Messages webhook (inbound messages + status): https://developers.facebook.com/documentation/business-messaging/whatsapp/webhooks/reference/messages</description></item>
/// <item><description>Message status webhook reference (pricing, errors, conversation): https://developers.facebook.com/documentation/business-messaging/whatsapp/webhooks/reference/messages/status</description></item>
/// <item><description>Groups API webhooks (group-facing statuses/pricing): https://developers.facebook.com/documentation/business-messaging/whatsapp/groups/webhooks</description></item>
/// </list>
/// </para>
/// </summary>
public class WebhookDTO
{
    public string Object { get; set; } = string.Empty;

    public IList<EntryDTO> Entry { get; set; } = new List<EntryDTO>();
}
