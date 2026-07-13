using System.Text.Json;
using BrilliantWhatsAppAPI.DTO;

namespace BrilliantWhatsAppAPI.Management;

public class WhatsAppManager
{
    public string BuildTextMessagePayload(string to , string message)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to ,
            type = "text" ,
            text = new
            {
                preview_url = false ,
                body = message
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    public string BuildTemplateMessagePayload(
        string to ,
        string templateName ,
        IList<tTemplateParameter> parameterList = null ,
        string languageCode = "en_US")
    {
        var parameters = parameterList is { Count: > 0 }
           ? parameterList.Select(p => new { type = p.Type , text = p.Text }).ToArray()
           : null;

        var payload = new
        {
            messaging_product = "whatsapp" ,
            to ,
            type = "template" ,
            template = new
            {
                name = templateName ,
                language = new
                {
                    code = languageCode
                } ,
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters
                        //parameters = new[]
                        //{
                        //    new { type = "text", text = "Brilliant" },
                        //    new { type = "text", text = "8523" }
                        //}
                    }
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }
}
