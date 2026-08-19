using FastEndpoints;
using WhatsAppData.DTO.Chat;
using WhatsAppData.Search.Chat;
using WhatsAppFDM.Chat;

namespace BrilliantWhatsAppAPI.Endpoints.Chat;

public class GetChatsEP : EndpointWithoutRequest<IList<ChatDTO>>
{
    ChatFDM _fdm = new ChatFDM();
    public override void Configure()
    {
        Get("/chats");
        AllowAnonymous();
    }

    public async override Task<IList<ChatDTO>> ExecuteAsync(CancellationToken ct)
    {
        return await _fdm.GetChatsContactList();
    }
}

public class GetChatHistoryEP : EndpointWithoutRequest<ChatHistoryDTO>
{
    private readonly ChatFDM _fdm = new ChatFDM();

    public override void Configure()
    {
        Get("chats/{waId}/messages");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get chat history / message thread for a specific WhatsApp ID";
            s.Params["waId"] = "The WhatsApp ID (waId) of the contact";
            s.Params["pageSize"] = "Number of messages to retrieve (optional)";
            s.Params["pageNumber"] = "Pagination page number (optional)";
        });
    }

    public override async Task<ChatHistoryDTO> ExecuteAsync(CancellationToken ct)
    {
        string waId = Route<string>("waId")!;
        int pageSize = Query<int?>("pageSize" , isRequired: false) ?? 10;
        int cursor = Query<int?>("pageNumber" , isRequired: false) ?? 1;

        return await _fdm.GetChatHistoryBy(new ChatHistorySH()
        {
            WaId = waId,
            PageSize = pageSize,
            PageNumber = cursor
        }, ct);
    }
}