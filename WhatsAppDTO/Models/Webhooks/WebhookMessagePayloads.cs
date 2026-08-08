using System.Text.Json.Serialization;

namespace WhatsAppDTO.Models.Webhooks;

public sealed class WebhookText
{
    public string Body { get; set; } = string.Empty;
}

public sealed class WebhookMedia
{
    public string? Id { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    public string? Sha256 { get; set; }

    public string? Caption { get; set; }

    public string? Filename { get; set; }

    public bool Voice { get; set; }
}

public sealed class WebhookButtonReply
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public sealed class WebhookListReply
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class WebhookInteractive
{
    public WebhookInteractiveType Type { get; set; }

    [JsonPropertyName("button_reply")]
    public WebhookButtonReply? ButtonReply { get; set; }

    [JsonPropertyName("list_reply")]
    public WebhookListReply? ListReply { get; set; }
}

public sealed class WebhookLocation
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }
}

public sealed class WebhookContactItem
{
    public WebhookContactName? Name { get; set; }

    public IList<WebhookContactPhone>? Phones { get; set; }
}

public sealed class WebhookContactName
{
    [JsonPropertyName("formatted_name")]
    public string FormattedName { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }
}

public sealed class WebhookContactPhone
{
    public string Phone { get; set; } = string.Empty;

    public string? Type { get; set; }
}

public sealed class WebhookReaction
{
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    public string? Emoji { get; set; }
}

public sealed class WebhookOrder
{
    [JsonPropertyName("catalog_id")]
    public string CatalogId { get; set; } = string.Empty;

    [JsonPropertyName("product_items")]
    public IList<WebhookOrderProductItem>? ProductItems { get; set; }

    public string? Text { get; set; }
}

public sealed class WebhookOrderProductItem
{
    [JsonPropertyName("product_retailer_id")]
    public string ProductRetailerId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [JsonPropertyName("item_price")]
    public double ItemPrice { get; set; }

    public string Currency { get; set; } = string.Empty;
}

public sealed class WebhookSystem
{
    public string? Body { get; set; }

    public string? Identity { get; set; }

    [JsonPropertyName("new_wa_id")]
    public string? NewWaId { get; set; }

    public string? Type { get; set; }
}
