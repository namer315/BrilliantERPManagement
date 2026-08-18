using FastEndpoints;
using WhatsAppData.DTO.Common;
using WhatsAppData.DTO.WhatsApp.FreeText;
using WhatsAppFDM.Chat;
using WhatsAppFDM.WhatsApp;

namespace BrilliantWhatsAppAPI.Endpoints.Chat;

public class GetChatsEP : EndpointWithoutRequest<IList<ContactDTO>>
{
    ChatFDM _fdm = new ChatFDM();
    public override void Configure()
    {
        Get("/chats");
        AllowAnonymous();
    }

    public async override Task<IList<ContactDTO>> ExecuteAsync(CancellationToken ct)
    {
        return await _fdm.GetChatsContactList();
    }
}
