using System.Text.Json;
using WhatsAppData.DTO.WhatsApp.Template;

namespace WhatsAppBusiness.WhatsApp;

internal class TemplatePayloadBuilderBE
{
    internal string BuildTemplateMessagePayload(TemplateSendDTO templateSend)
    {
        var parameters = templateSend.ParameterList is { Count: > 0 }
           ? templateSend.ParameterList.Select(p => new {
               type = p.Type ,
               parameter_name = p.Name ,
               text = p.Text
           }).ToArray()
           : null;

        var payload = new
        {
            messaging_product = "whatsapp" ,
            to = templateSend.RecipientPhoneNumber,
            type = "template" ,
            template = new
            {
                name = templateSend.TemplateName ,
                language = new
                {
                    code = templateSend.LanguageCode
                } ,
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters
                    }
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }
}
