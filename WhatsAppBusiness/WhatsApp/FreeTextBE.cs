using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class FreeTextBE : WhatsAppBE
{
    private FreeTextPayloadBuilderBE _payloadBuilder = new FreeTextPayloadBuilderBE();
    private MessageBE _message = new MessageBE();
    private ContactBE _contact = new ContactBE();

    public async Task<MessageResponseDTO> SendTextMessage(TextDTO text)
    {
        MessageVO message = await _message.GetNew(MessageVO.WhatsAppMessageTypes.Text);
        message.Content = text.Message;

        string s = await _message.Persist(message);

        string payload = _payloadBuilder.BuildTextMessagePayload(text);
        MessageResponseDTO messageResponseDTO = await PostAsync<MessageResponseDTO>("messages" , payload);

        //message.Status = messageResponseDTO.Messages[0]?.MessageStatus;
        message.MessageId = messageResponseDTO.Messages[0]?.Id;

        message.Receiver = await _contact.GetContactBy(messageResponseDTO.Contacts[0]?.WaId);
        if (string.IsNullOrEmpty(message.Receiver.PhoneNumber))
            message.Receiver.PhoneNumber = messageResponseDTO.Contacts[0]?.Input;
        message.UpdatedAt = DateTime.UtcNow;

        s = await _message.Persist(message, true);

        return messageResponseDTO;
    }
}
