using CommonData.Extensions;
using NHibernate.Engine;
using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Webhooks;
using WhatsAppData.VO.WhatsApp;
using static WhatsAppData.VO.WhatsApp.MessageStatusVO;

namespace WhatsAppBusiness.Webhooks;

public class WebhookBE
{
    private MessageBE _messageBE = new MessageBE();
    private MessageStatusBE _messageStatus = new MessageStatusBE();
    private ContactBE _contactBE = new ContactBE();
    private WhatsAppErrorBE _whatsAppErrorBE = new WhatsAppErrorBE();
    private WhatsAppPricingBE _whatsAppPricingBE = new WhatsAppPricingBE();

    public async Task<bool> HandleWebhook(WebhookDTO webhook)
    {
        try
        {
            //bool shouldSaveRequestToFile = false; // Set this flag based on your logic
            // Implementation for handling webhook
            string s;
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
                            if (change.Value.Statuses is { Count: > 0 })
                            {
                                foreach (StatusDTO status in change.Value.Statuses)
                                {
                                    MessageVO message = await _messageBE.getMessageBy(status.Id);
                                    MessageStatusVO messageStatus = _messageStatus.GetNew(message);
                                    messageStatus.Status = status.Status.ToEnum<WhatsAppMessageStatus>();
                                    messageStatus.Timestamp = Convert.ToInt64(status.Timestamp);

                                    s = await _messageStatus.Persist(messageStatus);

                                    if (status.Errors is { Count: > 0 })
                                    {
                                        foreach (ErrorDTO e in status.Errors)
                                        {
                                            WhatsAppErrorVO error = _whatsAppErrorBE.GetNew(messageStatus);
                                            error.ErrorCode = e.Code;
                                            error.Title = e.Title;
                                            error.Message = e.Message;
                                            error.Details = e.ErrorData?.Details ?? string.Empty;
                                            error.Href = e.Href;

                                            s = await _whatsAppErrorBE.Persist(error);
                                        }
                                    }
                                    if (status.Pricing is not null)
                                    {
                                        WhatsAppPricingVO pricing = _whatsAppPricingBE.GetNew(messageStatus);
                                        pricing.Billable = status.Pricing.Billable;
                                        pricing.PricingModel = status.Pricing.PricingModel;
                                        pricing.Type = status.Pricing.Type;
                                        pricing.Category = status.Pricing.Category;

                                        s = await _whatsAppPricingBE.Persist(pricing);

                                        // Pricing is informational (billable/model/category/type).
                                        // It is not currently mapped to a persistent column; log/capture for audit.
                                        Console.WriteLine($"[status] pricing id={status.Id} billable={status.Pricing.Billable} model={status.Pricing.PricingModel} type={status.Pricing.Type} category={status.Pricing.Category}");
                                    }
                                }
                            }

                            if (change.Value.Messages is { Count: > 0 })
                            {
                                foreach (MessageDTO msg in change.Value.Messages)
                                {

                                    MessageVO message = await _messageBE.GetNew(MessageVO.WhatsAppMessageTypes.Text, msg.Id);

                                    message.Sender = await _contactBE.GetContactBy(msg.From);
                                    if (string.IsNullOrEmpty(message.Sender.Name))
                                    {
                                        message.Sender.Name = change.Value.Contacts?.FirstOrDefault(x => x.WaId.Equals(message.Sender.WaId))?.Profile?.Name;
                                    }

                                    if(msg.Text is not null)
                                    {
                                        message.Content = msg.Text.Body;
                                    }
                                    message.Timestamp = Convert.ToInt64(msg.Timestamp);

                                    s = await _messageBE.Persist(message , true);
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
            throw ex;
            return true;
        }
    }
}
