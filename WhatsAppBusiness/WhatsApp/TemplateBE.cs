using FluentNHibernate.Conventions.Helpers;
using System.Text.Json;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class TemplateBE : WhatsAppBE
{
    private TemplatePayloadBuilderBE _payloadBuilder = new TemplatePayloadBuilderBE();
    private MessageBE _message = new MessageBE();
    private ContactBE contact = new ContactBE();


    public async Task<MessageResponseDTO> SendTemplateMessage(TemplateSendDTO templateSend)
    {
        if (templateSend.TemplateName.Equals("order_confirmed"))
            templateSend.LanguageCode = "en_US";

        MessageVO message = await _message.GetNew(MessageVO.WhatsAppMessageTypes.Template);
        message.Receiver = await contact.GetContactBy(templateSend.RecipientPhoneNumber);

        string s = await _message.Persist(message);

        string payload = _payloadBuilder.BuildTemplateMessagePayload(templateSend);
        string responce = await PostAsync("messages" , payload);

        MessageResponseDTO messageResponseDTO = JsonSerializer.Deserialize<MessageResponseDTO>(responce);
        message.Status = messageResponseDTO.Messages[0].MessageStatus;
        message.MessageId = messageResponseDTO.Messages[0].Id;
        s = await _message.Persist(message);

        return messageResponseDTO;
    }
}

