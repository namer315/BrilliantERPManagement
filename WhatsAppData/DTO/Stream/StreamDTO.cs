using System.Text.Json.Serialization;
using WhatsAppData.DTO.Chat;

namespace WhatsAppData.DTO.Stream;

public class StreamDTO
{
#if !DEBUG
    [JsonIgnore]
#endif
    public string Token { get; set; }

    public string TenantName { get; set; }

    public ChatHistoryDTO ChatHistory { get; set; }

    public ChatMessageDTO Message { get; set; }

    public ChatMessageStatusDTO MessageStatus { get; set; }
}
