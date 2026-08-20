using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class TemplateBE : WhatsAppBE
{
    private TemplatePayloadBuilderBE _payloadBuilder = new TemplatePayloadBuilderBE();
    private MessageBE _messageBE = new MessageBE();
    private ContactBE _contactBE = new ContactBE();


    public async Task<ChatMessageDTO> SendTemplateMessage(TemplateSendDTO templateSend)
    {
        //if (templateSend.TemplateName.Equals("order_confirmed"))
        //    templateSend.LanguageCode = "en_US";

        MessageVO message = await _messageBE.GetNew(MessageVO.WhatsAppMessageTypes.Template);
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

        string s = await _messageBE.Persist(message);

        string payload = _payloadBuilder.BuildTemplateMessagePayload(templateSend);
        MessageResponseDTO messageResponseDTO = await PostAsync<MessageResponseDTO>("messages" , payload);

        message.Status = messageResponseDTO.Messages[0]?.MessageStatus;
        message.MessageId = messageResponseDTO.Messages[0]?.Id;

        message.Receiver = await _contactBE.GetContactBy(messageResponseDTO.Contacts[0]?.WaId);
        if (string.IsNullOrEmpty(message.Receiver.PhoneNumber))
            message.Receiver.PhoneNumber = messageResponseDTO.Contacts[0]?.Input;
        //message.UpdatedAt = DateTime.UtcNow;

        s = await _messageBE.Persist(message, true);

        //return messageResponseDTO;
        ChatMessageDTO chatMessageDTO = new ChatMessageDTO();
        chatMessageDTO.Id = message.Id;
        chatMessageDTO.MessageId = message.MessageId;
        //chatMessageDTO.Timestamp = message.Timestamp;
        chatMessageDTO.MessageDirection = ChatMessageDTO.MessageDirections.Outgoing;
        chatMessageDTO.Body = message.Content;

        chatMessageDTO.Contact = new WhatsAppData.DTO.Common.ContactDTO();
        chatMessageDTO.Contact.Id = message.Receiver.Id;
        chatMessageDTO.Contact.WaId = message.Receiver.WaId;

        return chatMessageDTO;
    }
    public async Task<ChatMessageDTO> ResendFreeTextAsTemplateBy(string messageId, TemplateParameterDTO parameterCompany)
    {
        MessageVO message = await _messageBE.getMessageBy(messageId);

        TemplateSendDTO templateSend = new TemplateSendDTO();
        templateSend.TemplateName = "notification_confirmation";
        templateSend.RecipientPhoneNumber = message.Receiver.WaId;
        templateSend.ParameterList = new List<TemplateParameterDTO>()
        {
            new TemplateParameterDTO()
            {
                Type = "text" ,
                Text = message.Content ,
                Name = "text"
            },
            parameterCompany
        };

        ChatMessageDTO chatMessageDTO = await SendTemplateMessage(templateSend);
    
        return chatMessageDTO;
    }

    /// <summary>
    /// Fetches every message template provisioned under the configured WhatsApp Business Account.
    /// This is the canonical way to audit your template library — names, statuses, categories,
    /// languages, and component structures are all returned in a single call.
    /// </summary>
    /// <param name="fields">
    /// A comma-separated list of template properties to include in the response.
    /// Defaults to <c>"name,status,category,language,components"</c>.
    /// </param>
    /// <param name="limit">
    /// Maximum number of templates to return per page. The Cloud API caps this at 100;
    /// this method defaults to a conservative 50.
    /// </param>
    /// <param name="ct">A token to cancel the request mid-flight if the caller gives up.</param>
    /// <returns>
    /// The raw JSON response body from the Graph API as a string. Successful responses
    /// contain a <c>"data"</c> array whose elements represent individual template objects.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the internal WABA ID or access token has not been configured.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// Thrown when the API refuses the request — for example, an expired token (401),
    /// insufficient permissions (403), or a malformed URL (404).
    /// </exception>
    /// <exception cref="TaskCanceledException">
    /// Thrown when the request times out or the caller cancels via <paramref name="ct"/>.
    /// </exception>
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

