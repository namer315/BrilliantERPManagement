using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppBusiness.Webhooks;
using WhatsAppData.DTO.Webhooks;

namespace WhatsAppFDM;

public class WebhookFDM
{
    WebhookBE _be = new WebhookBE();
    public async Task<bool> HandleWebhook(WebhookDTO webhook)
    {
        return await _be.HandleWebhook(webhook);
    }

    public IAsyncEnumerable<dynamic> Stream(CancellationToken ct)
    {
        return _be.Stream(ct);
    }
}
