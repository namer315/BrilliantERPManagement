using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;

namespace WhatsAppFDM.WhatsApp;

public class TemplateFDM
{
    private TemplateBE _be = new TemplateBE();

    public async Task<MessageResponseDTO> SendTemplateMessage(TemplateSendDTO templateSend) => await _be.SendTemplateMessage(templateSend);
    
}
