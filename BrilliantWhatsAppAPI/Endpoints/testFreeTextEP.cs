using BrilliantWhatsAppAPI.DTO;
using BrilliantWhatsAppAPI.Management;
using FastEndpoints;

namespace BrilliantWhatsAppAPI.Endpoints;

public class testTextEP : Endpoint<tTextMessageDTO , dynamic>
{
    WhatsAppHelper _whatsAppHelper = new WhatsAppHelper();

    public override void Configure()
    {
        Post("test/Text");
        AllowAnonymous();
    }
    public async override Task<dynamic> ExecuteAsync(tTextMessageDTO req , CancellationToken ct)
    {
        return await _whatsAppHelper.SendMessageAsync(req);
        //return await _whatsAppHelper.SendImageMessageAsync(req);
    }
}
