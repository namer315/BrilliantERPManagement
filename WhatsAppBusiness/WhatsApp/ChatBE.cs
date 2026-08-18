using WhatsAppData.DAO;
using WhatsAppData.DTO.Chat;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class ChatBE
{
    public async Task<ChatHistoryDTO> GetChatHistoryBy(string waId)
    {
        //if (string.IsNullOrEmpty(waId))
        //    throw new ArgumentException("WhatsApp ID cannot be null or empty." , nameof(waId));

        if (string.IsNullOrEmpty(waId))
            throw new ArgumentNullException(nameof(waId) , "WhatsApp ID is required.");

        // Fetch message history from your data source
        IList<MessageVO> messageList = await new MessageDAO().GetMessageHistoryBy(waId);

        ChatHistoryDTO chatHistory = new ChatHistoryDTO();
        chatHistory.ChatMessagList = messageList.Select(x => new ChatMessageDTO()
        {
            Id = x.Id ,
            MessageId = x.MessageId ,
            Timestamp = x.Timestamp,
            Text = x.Content,
        })
            .ToList();


        return chatHistory;
    }
}
