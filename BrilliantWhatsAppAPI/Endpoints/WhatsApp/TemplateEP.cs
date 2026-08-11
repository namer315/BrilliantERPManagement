using BrilliantWhatsAppAPI.DTO;
using BrilliantWhatsAppAPI.Management;
using FastEndpoints;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints.WhatsApp;

public class TemplateEP : Endpoint<TemplateSendDTO , MessageResponseDTO>
{
    private TemplateFDM _fdm = new TemplateFDM();

    public override void Configure()
    {
        Post("WhatsApp/Template");
        AllowAnonymous();
    }
    public async override Task<MessageResponseDTO> ExecuteAsync(TemplateSendDTO req , CancellationToken ct)
    {
        return await _fdm.SendTemplateMessage(req);
    }
}
