using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageTypeDTO
{
    Text,
    Image,
    Video,
    Audio,
    Document,
    Sticker,
    Location,
    Contacts,
    Reaction,
    Interactive,
    Order,
    System,
    Button,
    Unsupported,
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusTypeDTO
{
    Delivered,
    Read,
    Sent,
    Failed,
    Deleted,
    Forwarded
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InteractiveTypeDTO
{
    [JsonPropertyName("button_reply")]
    ButtonReply,
    [JsonPropertyName("list_reply")]
    ListReply
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConversationOriginTypeDTO
{
    [JsonPropertyName("business_initiated")]
    BusinessInitiated,
    [JsonPropertyName("user_initiated")]
    UserInitiated,
    [JsonPropertyName("referral_conversion")]
    ReferralConversion
}
