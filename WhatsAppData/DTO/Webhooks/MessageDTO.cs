using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class MessageDTO
{
    public string From { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public string Timestamp { get; set; } = string.Empty;

    public MessageTypeDTO Type { get; set; }

    public ContextDTO? Context { get; set; }

    public IdentityDTO? Identity { get; set; }

    public TextDTO? Text { get; set; }

    public MediaDTO? Image { get; set; }

    public MediaDTO? Video { get; set; }

    public MediaDTO? Audio { get; set; }

    public MediaDTO? Document { get; set; }

    public MediaDTO? Sticker { get; set; }

    public InteractiveDTO? Interactive { get; set; }

    public LocationDTO? Location { get; set; }

    public IList<ContactItemDTO>? Contacts { get; set; }

    public ReactionDTO? Reaction { get; set; }

    public OrderDTO? Order { get; set; }

    public SystemDTO? System { get; set; }

    public ReferralDTO? Referral { get; set; }
}

public class ContextDTO
{
    public string From { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;
}

public class IdentityDTO
{
    public bool Acknowledged { get; set; }

    [JsonPropertyName("created_timestamp")]
    public string? CreatedTimestamp { get; set; }

    public string? Hash { get; set; }

    [JsonPropertyName("customer_identity_changed")]
    public bool CustomerIdentityChanged { get; set; }
}
