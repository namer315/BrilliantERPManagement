using FastEndpoints;
using BrilliantWhatsAppAPI.DTO;
using BrilliantWhatsAppAPI.Management;

namespace BrilliantWhatsAppAPI.Endpoints;

public class TemplateEP : Endpoint<tTemplateDTO , dynamic>
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
