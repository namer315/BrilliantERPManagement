using System.Text.Json;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;

namespace WhatsAppBusiness.WhatsApp;

public class TemplateBE : WhatsAppBE
{
    private TemplatePayloadBuilderBE _payloadBuilder = new TemplatePayloadBuilderBE();
    public async Task<MessageResponseDTO> SendTemplateMessage(TemplateSendDTO templateSend)
    {
        if (templateSend.TemplateName.Equals("order_confirmed"))
            templateSend.LanguageCode = "en_US";
        string payload = _payloadBuilder.BuildTemplateMessagePayload(templateSend);

        string responce = await PostAsync("messages" , payload);

        MessageResponseDTO messageResponseDTO = JsonSerializer.Deserialize<MessageResponseDTO>(responce);
        return messageResponseDTO;
    }
}

