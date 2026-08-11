using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// A single WhatsApp message template.
/// </summary>
public class MessageTemplateDTO
{
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("parameter_format")]
    public string ParameterFormat { get; set; } = string.Empty;

    public IList<TemplateComponentDTO> Components { get; set; } = [];

    public string Language { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("sub_category")]
    public string? SubCategory { get; set; }

    [JsonPropertyName("disable_ios_autofill")]
    public bool DisableIosAutofill { get; set; }

    [JsonPropertyName("is_primary_device_delivery_only")]
    public bool IsPrimaryDeviceDeliveryOnly { get; set; }

    [JsonPropertyName("library_template_name")]
    public string? LibraryTemplateName { get; set; }

    public string Id { get; set; } = string.Empty;
}
