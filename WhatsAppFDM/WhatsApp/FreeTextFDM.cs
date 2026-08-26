using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.FreeText;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.FreeText;

namespace WhatsAppFDM.WhatsApp;

public class FreeTextFDM
{
    FreeTextBE _be = new FreeTextBE();

    public async Task<SessionCheckResponseDTO> Check24hSession(string phone)
    {
        return await new MessageBE().Check24hSession(phone);
    }

    public async Task<ChatMessageDTO> SendDocumentMessage(DocumentDTO document)
    {
        return await _be.SendDocumentMessage(document);
    }

    //public async Task<MessageResponseDTO> SendFreeTextMessage(FreeTextDTO req)
    //{
    //    throw new NotImplementedException();
    //}

    public async Task<ChatMessageDTO> SendTextMessage(TextDTO text)
    {
        return await _be.SendTextMessage(text);
    }
}
