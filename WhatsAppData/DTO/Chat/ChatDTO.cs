using WhatsAppData.DTO.Common;

namespace WhatsAppData.DTO.Chat;

public class ChatDTO
{
    public string LastMessage { get; set; }
    public long? Timestamp { get; set; }
    public string MessageId { get; set; }

    public ContactDTO Contact { get; set; }
}
