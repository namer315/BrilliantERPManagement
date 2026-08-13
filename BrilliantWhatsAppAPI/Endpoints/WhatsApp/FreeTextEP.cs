using FastEndpoints;
using WhatsAppData.DTO.WhatsApp;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppData.DTO.WhatsApp.Template;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints.WhatsApp;

//Session messages
public class FreeTextEP : Endpoint<FreeTextDTO , MessageResponseDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();
    public override void Configure()
    {
        Post("Send/FreeText");
        AllowAnonymous();
    }

    public override Task<MessageResponseDTO> ExecuteAsync(FreeTextDTO req , CancellationToken ct)
    {
        return _fdm.SendFreeTextMessage(req);
    }
}
public class TextEP : Endpoint<TextDTO , MessageResponseDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();
    public override void Configure()
    {
        Post("Send/FreeText/Text");
        AllowAnonymous();
    }

    public override Task<MessageResponseDTO> ExecuteAsync(TextDTO req , CancellationToken ct)
    {
        return _fdm.SendTextMessage(req);
    }
}

