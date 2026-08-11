using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// A button within a BUTTONS component.
/// </summary>
public class TemplateButtonDTO
{
    public string Type { get; set; } = string.Empty;

    public string? Text { get; set; }

    public string? Url { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("offer_type")]
    public string? OfferType { get; set; }

    [JsonPropertyName("coupon_code")]
    public string? CouponCode { get; set; }

    public IList<string>? Example { get; set; }
}
