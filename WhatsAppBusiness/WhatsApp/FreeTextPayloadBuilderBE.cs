using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using WhatsAppData.DTO.WhatsApp.FreeText;

namespace WhatsAppBusiness.WhatsApp;

internal class FreeTextPayloadBuilderBE
{
    public string BuildTextMessagePayload(TextDTO text)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = text.PhoneNumber ,
            type = "text" ,
            text = new
            {
                preview_url = text.PreviewURL ,
                body = text.Message
            }
        };

        return JsonSerializer.Serialize(payload);
    }
}
