using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Chat;

public class ChatMessageDTO
{
    public Guid Id { get; set; }
    public string MessageId { get; set; }

    [JsonIgnore]
    public long? Timestamp { get; set; }
    public DateTimeOffset? DateTimeUTC => Timestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(Timestamp.Value) : null;

    //public bool HasPreviewUrl { get; init; } = false;

    public MessageDirections MessageDirection { get; set; }


    public string Body { get; set; }


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageDirections
    {
        Incoming,
        Outgoing
    }
}
