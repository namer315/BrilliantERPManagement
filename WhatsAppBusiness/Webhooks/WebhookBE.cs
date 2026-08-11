using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.DTO.Webhooks;

namespace WhatsAppBusiness.Webhooks;

public class WebhookBE
{
    public async Task<bool> HandleWebhook(WebhookDTO webhook)
    {
        //bool shouldSaveRequestToFile = false; // Set this flag based on your logic
        // Implementation for handling webhook

        if (!webhook.Object.Equals("whatsapp_business_account") || webhook.Entry is not { Count: > 0 })
            return true;

        foreach (EntryDTO entry in webhook.Entry)
        {
            if (entry.Changes is not { Count: > 0 })
                return true;

            foreach (var change in entry.Changes)
            {
                if (change.Value is null)
                    return true;

                // Handle the change based on its type
                switch (change.Field)
                {
                    case "messages":
                        // Handle incoming messages
                        break;
                    case "statuses":
                        // Handle message status updates
                        break;
                    case "message_template_status_update":
                        // Handle message template status update
                        break;
                    default:
                        // Handle other types of changes if needed
                        break;
                }
            }
        }

        return false;
    }
}
