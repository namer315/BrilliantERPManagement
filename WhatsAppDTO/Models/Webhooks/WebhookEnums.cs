using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookObjectType
{
    [JsonPropertyName("whatsapp_business_account")]
    WhatsAppBusinessAccount
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookMessageType
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
public enum WebhookStatusType
{
    Delivered,
    Read,
    Sent,
    Failed,
    Deleted,
    Forwarded
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookInteractiveType
{
    [JsonPropertyName("button_reply")]
    ButtonReply,
    [JsonPropertyName("list_reply")]
    ListReply
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookConversationOriginType
{
    [JsonPropertyName("business_initiated")]
    BusinessInitiated,
    [JsonPropertyName("user_initiated")]
    UserInitiated,
    [JsonPropertyName("referral_conversion")]
    ReferralConversion
}
