using System.Text.Json.Serialization;
using WhatsAppData.DTO.Chat;

namespace WhatsAppData.DTO.Stream;

public class StreamDTO
{
#if !DEBUG
    [JsonIgnore]
#endif
    public string Token { get; set; }

    public string TenentName { get; set; }

    public ChatHistoryDTO ChatHistory { get; set; }

    public StreamMessageDTO Message { get; set; }
}
