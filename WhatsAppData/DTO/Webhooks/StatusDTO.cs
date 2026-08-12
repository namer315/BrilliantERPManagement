using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class StatusDTO
{
    public string Id { get; set; } = string.Empty;

    public string Status { get; set; }

    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("recipient_id")]
    public string RecipientId { get; set; } = string.Empty;

    /// <summary>
    /// Only included if the message was sent to a group. Values: "individual" or "group".
    /// </summary>
    [JsonPropertyName("recipient_type")]
    public string RecipientType { get; set; } = string.Empty;

    /// <summary>
    /// Only included if the message was sent to a group (identifies the group participant's phone).
    /// </summary>
    [JsonPropertyName("recipient_participant_id")]
    public string RecipientParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// Only included if the identity change check is enabled.
    /// </summary>
    [JsonPropertyName("recipient_identity_key_hash")]
    public string RecipientIdentityKeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Only included if the message was sent with biz_opaque_callback_data.
    /// Free-form business data echoed back for campaign/audit tracking.
    /// </summary>
    [JsonPropertyName("biz_opaque_callback_data")]
    public string BizOpaqueCallbackData { get; set; } = string.Empty;

    public ConversationDTO Conversation { get; set; }
    /// <summary>
    /// only included with sent status, and one of either delivered or read status
    /// </summary>
    public PricingDTO Pricing { get; set; }
    /// <summary>
    /// only included if failure to send or deliver message
    /// </summary>
    public IList<ErrorDTO> Errors { get; set; }
}

public class ConversationDTO
{
    public string Id { get; set; } = string.Empty;

    public ConversationOriginDTO Origin { get; set; }

    [JsonPropertyName("expiration_timestamp")]
    public string ExpirationTimestamp { get; set; } = string.Empty;
}

public class ConversationOriginDTO
{
    public ConversationOriginTypeDTO Type { get; set; }
}

/// <summary>
/// Pricing information for the message status.
/// Only included with sent status, and one of either delivered or read status.
/// <para>
/// Related documentation:
/// <list type="bullet">
/// <item><description>Pricing overview (per-message pricing, rates, volume tiers): https://developers.facebook.com/documentation/business-messaging/whatsapp/pricing</description></item>
/// <item><description>Groups API pricing (billing per recipient, group pricing categories): https://developers.facebook.com/documentation/business-messaging/whatsapp/groups/pricing</description></item>
/// <item><description>Groups API webhooks (pricing data in status webhooks): https://developers.facebook.com/documentation/business-messaging/whatsapp/groups/webhooks#pricing-information</description></item>
/// <item><description>Customer service windows: https://developers.facebook.com/documentation/business-messaging/whatsapp/messages/send-messages#customer-service-windows</description></item>
/// <item><description>Analytics (pricing_analytics, tiering): https://developers.facebook.com/documentation/business-messaging/whatsapp/analytics#pricing-analytics</description></item>
/// </list>
/// </para>
/// </summary>
public class PricingDTO
{
    public bool? Billable { get; set; }

    [JsonPropertyName("pricing_model")]
    public string PricingModel { get; set; } = string.Empty;

    /// <summary>
    /// PRICING_TYPE (e.g. "CBP", "CBP_NUMBER", "GPU", "0").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}
