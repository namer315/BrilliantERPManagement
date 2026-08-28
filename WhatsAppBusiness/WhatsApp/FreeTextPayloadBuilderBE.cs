using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using WhatsAppData.DTO.FreeText;
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
    public string BuildTextMessagePayload(FreeTextDTO freeText)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = freeText.Phone ,
            type = "text" ,
            text = new
            {
                preview_url = freeText.Text.PreviewURL ,
                body = freeText.Body
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    public string BuildDocumentMessagePayload(FreeTextDTO freeText)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = freeText.Phone ,
            type = "document" ,
            document = new
            {
                id = freeText.Document.Id ,
                filename = freeText.Document.FileName ,
                caption = freeText.Body
            }
        };
        return JsonSerializer.Serialize(payload);
    }

    public string BuildImageMessagePayload(FreeTextDTO freeText)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = freeText.Phone ,
            type = "image" ,
            image = new
            {
                id = freeText.Image.Id ,
                caption = freeText.Body
            }
        };
        return JsonSerializer.Serialize(payload);
    }

    public string BuildVideoMessagePayload(FreeTextDTO freeText)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = freeText.Phone ,
            type = "video" ,
            video = new
            {
                id = freeText.Video.Id ,
                caption = freeText.Body
            }
        };
        return JsonSerializer.Serialize(payload);
    }

    public string BuildAudioMessagePayload(FreeTextDTO freeText)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = freeText.Phone ,
            type = "audio" ,
            audio = new
            {
                id = freeText.Audio.Id
            }
        };
        return JsonSerializer.Serialize(payload);
    }
}
