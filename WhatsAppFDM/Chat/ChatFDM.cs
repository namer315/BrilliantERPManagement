using WhatsAppBusiness.WhatsApp;
using WhatsAppData.DTO.Chat;
using WhatsAppData.Search.Chat;

namespace WhatsAppFDM.Chat;

public class ChatFDM
{
    private ChatBE _be = new ChatBE();
    public async Task<ChatHistoryDTO> GetChatHistoryBy(ChatHistorySH chatHistory , CancellationToken ct)
    {
        return await _be.GetChatHistoryBy(chatHistory);
    }

    public async Task<IList<ChatDTO>> GetChatsContactList()
    {
        return await _be.GetChatsContactList();
    }
}
