using FastEndpoints;
using WhatsAppData.DAO;
using WhatsAppData.DTO.Chat;
using WhatsAppData.VO.WhatsApp;
using WhatsAppFDM.Chat;
using WhatsAppFDM.Messages;

namespace BrilliantWhatsAppAPI.Endpoints.Messages;

public class MessageEP
{
}
/*public class GetMessageByIdEP : EndpointWithoutRequest<ChatMessageDTO>
{
    private readonly MessageFDM _fdm = new MessageFDM();

    public override void Configure()
    {
        Get("Messages/{messageId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get a single message by its WhatsApp message ID";
            s.Params["messageId"] = "The WhatsApp message ID to look up";
        });
    }

    public override async Task<ChatMessageDTO> ExecuteAsync(CancellationToken ct)
    {
        string messageId = Route<string>("messageId")!;

        return await _fdm.GetMessageById(messageId);
    }
}*/

public class GetMessageByIdEP : EndpointWithoutRequest<ChatMessageDTO>
{
    private readonly MessageFDM _fdm = new MessageFDM();

    public override void Configure()
    {
        Get("Messages/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get a single message by its internal Id";
            s.Params["id"] = "The Guid Id of the message to look up";
        });
    }

    public override async Task<ChatMessageDTO> ExecuteAsync(CancellationToken ct)
    {
        Guid id = Route<Guid>("id");

        return await _fdm.GetMessageById(id);
    }
}