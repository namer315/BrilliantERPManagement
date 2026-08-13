using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class TemplateBE : WhatsAppBE
{
    private TemplatePayloadBuilderBE _payloadBuilder = new TemplatePayloadBuilderBE();
    private MessageBE _message = new MessageBE();
    private ContactBE _contact = new ContactBE();


    public async Task<MessageResponseDTO> SendTemplateMessage(TemplateSendDTO templateSend)
    {
        //if (templateSend.TemplateName.Equals("order_confirmed"))
        //    templateSend.LanguageCode = "en_US";

        MessageVO message = await _message.GetNew(MessageVO.WhatsAppMessageTypes.Template);
        //message.Receiver = await _contact.GetContactBy(templateSend.RecipientPhoneNumber);
        //message_templates?name=order_confirmed
        TemplatesResponseDTO templatesResponseDTO = await GetAllTemplatesAsync(templateName: templateSend.TemplateName);

        if(templatesResponseDTO.Data is not { Count:>0} || templatesResponseDTO.Data[0].Components is not { Count: > 0 })
            throw new InvalidOperationException( $"Template '{templateSend.TemplateName}' was not found or contains no components.");

        message.Content = templatesResponseDTO.Data[0].Components[0].Text;
        templateSend.LanguageCode = templatesResponseDTO.Data[0].Language;
        foreach (TemplateParameterDTO parameter in templateSend.ParameterList)
        {
            message.Content = message.Content.Replace("{{" + parameter.Name + "}}" , parameter.Text);
        }

        string s = await _message.Persist(message);

        string payload = _payloadBuilder.BuildTemplateMessagePayload(templateSend);
        MessageResponseDTO messageResponseDTO = await PostAsync<MessageResponseDTO>("messages" , payload);

        message.Status = messageResponseDTO.Messages[0]?.MessageStatus;
        message.MessageId = messageResponseDTO.Messages[0]?.Id;

        message.Receiver = await _contact.GetContactBy(messageResponseDTO.Contacts[0]?.WaId);
        if (string.IsNullOrEmpty(message.Receiver.PhoneNumber))
            message.Receiver.PhoneNumber = messageResponseDTO.Contacts[0]?.Input;
        message.UpdatedAt = DateTime.UtcNow;

        s = await _message.Persist(message, true);

        return messageResponseDTO;
    }


    public async Task<TemplatesResponseDTO> GetAllTemplatesAsync(
       string fields = "name,status,category,language,components" ,
       int limit = 50 ,
       string templateName = null,
       CancellationToken ct = default)
    {
        //// --- Input validation ---
        //if (string.IsNullOrWhiteSpace(_WhatsAppBusinessAccountId))
        //    throw new ArgumentNullException(nameof(_WhatsAppBusinessAccountId) , "WABA ID cannot be null or empty.");

        //if (string.IsNullOrWhiteSpace(_accessToken))
        //    throw new ArgumentNullException(nameof(_accessToken) , "Access token cannot be null or empty.");

        //// --- Build the URL ---
        //var url = $"https://graph.facebook.com/v22.0/{_WhatsAppBusinessAccountId}/message_templates"
        //        + $"?fields={Uri.EscapeDataString(fields)}"
        //        + $"&limit={limit}";

        //string respond = await _httpClientHelper.GetAsync(url);
        //string responce = await GetWABAAsync("message_templates?fields={Uri.EscapeDataString(fields)");

        //TemplatesResponseDTO templates = JsonSerializer.Deserialize<TemplatesResponseDTO>(respond)
        //    ?? throw new InvalidOperationException("Failed to deserialize WhatsApp template response.");

        string queryParameters = $"fields={Uri.EscapeDataString(fields)}&limit={limit}";
        if (!string.IsNullOrEmpty(templateName))
        {
            queryParameters += $"&name={Uri.EscapeDataString(templateName)}";
        }

        TemplatesResponseDTO templatesResponseDTO = await GetWABAAsync<TemplatesResponseDTO>($"message_templates?{queryParameters}");

        return templatesResponseDTO;
    }
}

