namespace WhatsAppData.DTO.Chat;

public class ChatHistoryDTO
{
    public IList<ChatMessageDTO> ChatMessagList { get; set; }

    public ChatDTO Chat { get; set; }
}
