using FastEndpoints;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints;

public class TemplateListEP : EndpointWithoutRequest<TemplatesResponseDTO>
{
    private TemplateFDM _fdm = new TemplateFDM();
    public override void Configure()
    {
        Get("Template/List");
        AllowAnonymous();
    }

    public async override Task<TemplatesResponseDTO> ExecuteAsync(CancellationToken ct)
    {
        return await _fdm.GetTemplateList();
    }
}
