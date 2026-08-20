using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;

namespace WhatsAppFDM.WhatsApp;

public class TemplateFDM
{
    private TemplateBE _be = new TemplateBE();

    public async Task<TemplatesResponseDTO> GetTemplateList()
    {
        return await _be.GetAllTemplatesAsync();
    }

    public async Task<ChatMessageDTO> SendTemplateMessage(TemplateSendDTO templateSend) => await _be.SendTemplateMessage(templateSend);

    public async Task<ChatMessageDTO> ResendFreeTextAsTemplateBy(string messageId , TemplateParameterDTO req)
        => await new TemplateBE().ResendFreeTextAsTemplateBy(messageId , req);
    
}
