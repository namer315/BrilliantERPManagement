using System.Text.Json.Serialization;
using WhatsAppData.DTO.Common;
using static WhatsAppData.VO.WhatsApp.MessageStatusVO;
using static WhatsAppData.VO.WhatsApp.MessageVO;

namespace WhatsAppData.DTO.Chat;

public class ChatMessageDTO
{
    public Guid Id { get; set; }
    public string MessageId { get; set; }

    public WhatsAppMessageTypes Type { get; set; }

    [JsonIgnore]
    public long? Timestamp { get; set; }
    public DateTimeOffset? DateTimeUTC => Timestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(Timestamp.Value) : null;

    //public bool HasPreviewUrl { get; init; } = false;

    public MessageDirections MessageDirection { get; set; }

    public WhatsAppMessageStatus? Status { get; set; }

    public string Body { get; set; }


    public ContactDTO Contact { get; set; }

    public ChatMessageButtonDTO Button { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageDirections
    {
        Incoming,
        Outgoing
    }
}
