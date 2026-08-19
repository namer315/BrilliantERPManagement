using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Chat;
using WhatsAppData.DTO.Common;

namespace WhatsAppFDM.Chat;

public class ChatFDM
{
    private ChatBE _be = new ChatBE();
    public async Task<ChatHistoryDTO> GetChatHistoryBy(string waId , string? cursor , int pageSize , CancellationToken ct)
    {
        return await _be.GetChatHistoryBy(waId);
    }

    public async Task<IList<ChatDTO>> GetChatsContactList()
    {
        return await _be.GetChatsContactList();
    }
}
