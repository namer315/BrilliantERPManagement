using FastEndpoints;
using WhatsAppData.DTO.Template;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints;

public class TemplateListEP : EndpointWithoutRequest<TemplatesDTO>
{
    private TemplateFDM _fdm = new TemplateFDM();
    public override void Configure()
    {
        Get("Template/List");
        AllowAnonymous();
    }

    public async override Task<TemplatesDTO> ExecuteAsync(CancellationToken ct)
    {
        return await _fdm.GetTemplateList();
    }
}
