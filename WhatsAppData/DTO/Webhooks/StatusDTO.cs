using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class StatusDTO
{
    public string Id { get; set; } = string.Empty;

    public StatusTypeDTO Status { get; set; }

    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("recipient_id")]
    public string RecipientId { get; set; } = string.Empty;

    public ConversationDTO? Conversation { get; set; }

    public PricingDTO? Pricing { get; set; }

    public IList<ErrorDTO>? Errors { get; set; }
}

public class ConversationDTO
{
    public string Id { get; set; } = string.Empty;

    public ConversationOriginDTO? Origin { get; set; }

    [JsonPropertyName("expiration_timestamp")]
    public string? ExpirationTimestamp { get; set; }
}

public class ConversationOriginDTO
{
    public ConversationOriginTypeDTO Type { get; set; }
}

public class PricingDTO
{
    public bool Billable { get; set; }

    [JsonPropertyName("pricing_model")]
    public string PricingModel { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}
