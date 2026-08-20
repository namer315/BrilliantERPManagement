using FastEndpoints;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints.Send;

public class TemplateEP : Endpoint<TemplateSendDTO , ChatMessageDTO>
{
    private TemplateFDM _fdm = new TemplateFDM();

    public override void Configure()
    {
        Post("Send/Template/Text");
        AllowAnonymous();
    }
    public async override Task<ChatMessageDTO> ExecuteAsync(TemplateSendDTO req , CancellationToken ct)
    {
        return await _fdm.SendTemplateMessage(req);
    }
}
public class TemplateListEP : EndpointWithoutRequest<TemplatesResponseDTO>
{
    private TemplateFDM _fdm = new TemplateFDM();
    public override void Configure()
    {
        Get("Receive/Template/List");
        AllowAnonymous();
    }

    public async override Task<TemplatesResponseDTO> ExecuteAsync(CancellationToken ct)
    {
        return await _fdm.GetTemplateList();
    }
}