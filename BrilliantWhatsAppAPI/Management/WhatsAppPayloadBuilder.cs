using BrilliantWhatsAppAPI.DTO;
using System.Text.Json;

namespace BrilliantWhatsAppAPI.Management;

public class WhatsAppPayloadBuilder
{
    public string BuildTextMessagePayload(tTextMessageDTO req)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = req.PhoneNumber ,
            type = "text" ,
            text = new
            {
                preview_url = req.PreviewURL ,
                body = req.Message
            }
        };

        return JsonSerializer.Serialize(payload);
    }


    /*public string BuildInteractiveButtonPayload(tTextMessageDTO req)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = req.PhoneNumber ,
            type = "interactive" ,
            interactive = new
            {
                type = "button" ,
                body = new
                {
                    text = req.Message
                } ,
                action = new
                {
                    buttons = new[]
                    {
                    new {
                        type = "reply",
                        reply = new {
                            id = "btn1",
                            title = "Confirm"
                        }
                    },
                    new {
                        type = "reply",
                        reply = new {
                            id = "btn2",
                            title = "Cancel"
                        }
                    }
                }
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }
    */

    // Builds the JSON body for sending an image message by media ID.
    // Image (optionally with caption text under it)
    // Image (optionally with caption text under it)
    public string BuildImageMessagePayload(tTextMessageDTO req , string mediaId)
    {
        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = req.PhoneNumber ,
            type = "image" ,
            image = new
            {
                id = mediaId ,
                caption = req.Message   // optional caption — covers "image with text"
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    // Interactive — supports any combination of header(image) + body(text) + buttons
    public string BuildInteractiveMessagePayload(tTextMessageDTO req , string mediaId = null)
    {
        var buttons = req.ButtonList?
            .Where(b => b.Reply != null)
            .Select(b => new
            {
                type = b.Type.ToString().ToLowerInvariant() ,   // "reply"
                reply = new
                {
                    id = b.Reply.Id ?? Guid.NewGuid().ToString("N") ,
                    title = b.Reply.Title
                }
            })
            .ToArray() ?? [];

        var interactive = new Dictionary<string , object?>
        {
            ["type"] = "button" ,
            ["body"] = new { text = req.Message }
        };

        // Optional image header — enables "image + text + buttons"
        if (!string.IsNullOrEmpty(mediaId))
        {
            interactive["header"] = new
            {
                type = "image" ,
                image = new { id = mediaId }
            };
        }

        interactive["action"] = new { buttons };

        var payload = new
        {
            messaging_product = "whatsapp" ,
            recipient_type = "individual" ,
            to = req.PhoneNumber ,
            type = "interactive" ,
            interactive
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
           ? parameterList.Select(p => new { 
               type = p.Type ,
               parameter_name = p.Name ,
               text = p.Text  
           }).ToArray()
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
