using FastEndpoints;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.FreeText;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints.Messages;

//Session messages
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
public class PhotoEP : Endpoint<PhotoDTO , ChatMessageDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();

    public override void Configure()
    {
        Post("Messages/FreeText/Photo");
        AllowAnonymous();
    }

    public override Task<ChatMessageDTO> ExecuteAsync(PhotoDTO req , CancellationToken ct)
    {
        return _fdm.SendImageMessage(req);
    }
}

public class VideoEP : Endpoint<VideoDTO , ChatMessageDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();

    public override void Configure()
    {
        Post("Messages/FreeText/Video");
        AllowAnonymous();
    }

    public override Task<ChatMessageDTO> ExecuteAsync(VideoDTO req , CancellationToken ct)
    {
        return _fdm.SendVideoMessage(req);
    }
}

public class AudioEP : Endpoint<AudioDTO , ChatMessageDTO>
{
    FreeTextFDM _fdm = new FreeTextFDM();

    public override void Configure()
    {
        Post("Messages/FreeText/Audio");
        AllowAnonymous();
    }

    public override Task<ChatMessageDTO> ExecuteAsync(AudioDTO req , CancellationToken ct)
    {
        return _fdm.SendAudioMessage(req);
    }
}

