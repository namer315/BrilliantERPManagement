using System.Text.Json.Serialization;
using WhatsAppData.DTO.Common;
using static WhatsAppData.VO.WhatsApp.MessageStatusVO;

namespace WhatsAppData.DTO.Chat;

public class ChatMessageStatusDTO
{
    public string MessageId { get; set; }     // WhatsApp message ID
    public WhatsAppMessageStatus Status { get; set; }   // e.g. "accepted", "delivered"

    [JsonIgnore]
    public long Timestamp { get; set; }        // raw webhook timestamp (Unix seconds)
    public DateTimeOffset? DateTimeUTC => DateTimeOffset.FromUnixTimeSeconds(Timestamp);


    public ContactDTO Contact { get; set; }
    public ErrorDTO Error { get; set; }  // Optional error details if the status indicates a failure
}
