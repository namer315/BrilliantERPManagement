using FastEndpoints;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppFDM.Chat;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints.Messages;

public class TemplateEP
{
}

public class ResendTemplateEP : Endpoint<TemplateParameterDTO, ChatMessageDTO>
{
    private readonly TemplateFDM _fdm = new TemplateFDM();

    public override void Configure()
    {
        Post("Messages/Templates/{messageId}/");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Resend a WhatsApp free text as a template message by original Message ID";
            s.Params["messageId"] = "The unique Message ID of the template message to resend";
        });
    }

    public override async Task<ChatMessageDTO> ExecuteAsync(TemplateParameterDTO req, CancellationToken ct)
    {
        string messageId = Route<string>("messageId")!;
        //string templateId = Query<string>("templateId" , isRequired: true)!;

        return await _fdm.ResendFreeTextAsTemplateBy(messageId, req);
    }
}