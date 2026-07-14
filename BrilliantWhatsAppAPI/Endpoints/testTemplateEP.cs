using BrilliantWhatsAppAPI.DTO;
using BrilliantWhatsAppAPI.Management;
using FastEndpoints;

namespace BrilliantWhatsAppAPI.Endpoints;

public class testTemplateEP : Endpoint<tTemplateDTO , dynamic>
{
    WhatsAppHelper _whatsAppHelper = new WhatsAppHelper();

    public override void Configure()
    {
        Post("test/Template");
        AllowAnonymous();
    }
    public async override Task<dynamic> ExecuteAsync(tTemplateDTO req , CancellationToken ct)
    {
        return await _whatsAppHelper.SendTemplateMessageAsync(req.PhoneNumber , req.TemplateName , req.ParameterList, req.LanguageCode);
    }
}
public class GetTemplatesEP : EndpointWithoutRequest<WhatsAppTemplateResponse>
{
    WhatsAppHelper _whatsAppHelper = new WhatsAppHelper();
    public override void Configure()
    {
        Get("test/TemplateList");
        AllowAnonymous();
    }

    public async override Task<WhatsAppTemplateResponse> ExecuteAsync(CancellationToken ct)
    {
        return await _whatsAppHelper.GetAllTemplatesAsync();
    }
}

