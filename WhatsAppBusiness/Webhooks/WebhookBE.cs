using CommonData.Extensions;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Webhooks;
using WhatsAppData.VO.WhatsApp;
using static WhatsAppData.VO.WhatsApp.MessageStatusVO;

namespace WhatsAppBusiness.Webhooks;

public class WebhookBE
{
    private MessageBE _message = new MessageBE();
    private MessageStatusBE _messageStatus = new MessageStatusBE();
    private ContactBE _contact = new ContactBE();
    private WhatsAppErrorBE _whatsAppErrorBE = new WhatsAppErrorBE();

    public async Task<bool> HandleWebhook(WebhookDTO webhook)
    {
        try
        {
            //bool shouldSaveRequestToFile = false; // Set this flag based on your logic
            // Implementation for handling webhook

            if (!webhook.Object.Equals("whatsapp_business_account") || webhook.Entry is not { Count: > 0 })
                return true;

            foreach (EntryDTO entry in webhook.Entry)
            {
                if (entry.Changes is not { Count: > 0 })
                    return true;

                foreach (ChangeDTO change in entry.Changes)
                {
                    if (change.Value is null)
                        return true;

                    // Handle the change based on its type
                    switch (change.Field)
                    {
                        case "messages":
                        {
                            // Handle incoming messages
                            foreach (StatusDTO status in change.Value.Statuses)
                            {
                                MessageVO message = await _message.getMessageBy(status.Id);
                                MessageStatusVO messageStatus = _messageStatus.GetNew(message);
                                messageStatus.Status = status.Status.ToEnum<WhatsAppMessageStatus>();
                                messageStatus.Timestamp = Convert.ToInt64(status.Timestamp);

                                string s = await _messageStatus.Persist(messageStatus);

                                foreach (ErrorDTO e in status.Errors)
                                {
                                    WhatsAppErrorVO error = _whatsAppErrorBE.GetNew(messageStatus);
                                    error.ErrorCode = e.Code;
                                    error.Title = e.Title;
                                    error.Message = e.Message;
                                    error.Details = e.ErrorData.Details;
                                    error.Href = e.Href;
                                }
                                if (status.Pricing is not null)
                                {

                                }
                            }
                        }
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
        catch(Exception ex)
        {
            return true;
        }
    }
}
