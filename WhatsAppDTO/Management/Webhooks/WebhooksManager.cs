using WhatsAppBusiness.Webhooks;
using WhatsAppData.DTO.Webhooks;

namespace WhatsAppDTO.Management.Webhooks;

public class WebhooksManager : ManagerBase
{
    private readonly WebhookBE _be = new WebhookBE();
    public async Task<bool> HandleWebhookRequest(WebhookDTO req)
    {
        try
        {
            return await _be.HandleWebhook(req);
        }
        catch(Exception ex)
        {
            return true;
        }
    }
}
