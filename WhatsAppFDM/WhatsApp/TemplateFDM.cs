using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppBusiness.WhatsApp;

namespace WhatsAppFDM.WhatsApp;

public class TemplateFDM
{
    private TemplateBE _be = new TemplateBE();

    public async Task<MessageResponseDTO> SendTemplateMessage(TemplateSendDTO templateSend)
    {
        return await _be.SendTemplateMessage(templateSend);
    }
}
