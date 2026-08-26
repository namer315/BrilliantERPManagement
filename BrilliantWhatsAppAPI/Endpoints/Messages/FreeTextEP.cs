using FastEndpoints;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.FreeText;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints.Messages;

//Session messages
//public class FreeTextEP : Endpoint<FreeTextDTO , MessageResponseDTO>
//{
//    FreeTextFDM _fdm = new FreeTextFDM();
//    public override void Configure()
//    {
//        Post("Send/FreeText");
//        AllowAnonymous();
//    }

//    public override Task<MessageResponseDTO> ExecuteAsync(FreeTextDTO req , CancellationToken ct)
//    {
//        return _fdm.SendFreeTextMessage(req);
//    }
//}
//public class GetLastMessageBySender : Endpoint<SessionCheckRequestDTO , SessionCheckResponseDTO>
//{
//    FreeTextFDM _fdm = new FreeTextFDM();
//    public override void Configure()
//    {
//        Post("Send/FreeText/IsIn24hSession");
//        AllowAnonymous();
//    }

//    public override Task<SessionCheckResponseDTO> ExecuteAsync(SessionCheckRequestDTO req , CancellationToken ct)
//    {
//        return _fdm.Check24hSession(req.PhoneNumber);
//    }
//}
public class TextEP : Endpoint<TextDTO , ChatMessageDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();
    public override void Configure()
    {
        Post("Messages/FreeText/Text");
        AllowAnonymous();
    }

    public override Task<ChatMessageDTO> ExecuteAsync(TextDTO req , CancellationToken ct)
    {
        return _fdm.SendTextMessage(req);
    }
}
public class DocumentEP : Endpoint<DocumentDTO , ChatMessageDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();

    public override void Configure()
    {
        Post("Messages/FreeText/Document");
        AllowAnonymous();
    }

    public override Task<ChatMessageDTO> ExecuteAsync(DocumentDTO req , CancellationToken ct)
    {
        return _fdm.SendDocumentMessage(req);
    }
}

