using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class TextDTO
{
    public string Body { get; set; } = string.Empty;
}

public class MediaDTO
{
    public string? Id { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    public string? Caption { get; set; }

    public string? Filename { get; set; }

    public bool Voice { get; set; }
}

public class InteractiveDTO
{
    public InteractiveTypeDTO Type { get; set; }

    [JsonPropertyName("button_reply")]
    public ButtonReplyDTO? ButtonReply { get; set; }

    [JsonPropertyName("list_reply")]
    public ListReplyDTO? ListReply { get; set; }
}

public class ButtonReplyDTO
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ListReplyDTO
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class LocationDTO
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }
}

public class ContactItemDTO
{
    public ContactNameDTO? Name { get; set; }

    public IList<ContactPhoneDTO>? Phones { get; set; }
}

public class ContactNameDTO
{
    [JsonPropertyName("formatted_name")]
    public string FormattedName { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
}

public class ContactPhoneDTO
{
    public string Phone { get; set; } = string.Empty;

    public string? Type { get; set; }
}

public class ReactionDTO
{
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    public string? Emoji { get; set; }
}

public class OrderDTO
{
    [JsonPropertyName("catalog_id")]
    public string CatalogId { get; set; } = string.Empty;

    [JsonPropertyName("product_items")]
    public IList<OrderProductItemDTO>? ProductItems { get; set; }

    public string? Text { get; set; }
}

public class OrderProductItemDTO
{
    [JsonPropertyName("product_retailer_id")]
    public string ProductRetailerId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [JsonPropertyName("item_price")]
    public double ItemPrice { get; set; }

    public string Currency { get; set; } = string.Empty;
}

public class SystemDTO
{
    public string? Body { get; set; }

    public string? Type { get; set; }

    [JsonPropertyName("new_wa_id")]
    public string? NewWaId { get; set; }
}

public class ReferralDTO
{
    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("source_type")]
    public string? SourceType { get; set; }

    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    public string? Headline { get; set; }

    public string? Body { get; set; }
}
