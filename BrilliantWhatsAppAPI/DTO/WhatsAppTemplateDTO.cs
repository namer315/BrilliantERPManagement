using System.Text.Json.Serialization;

namespace BrilliantWhatsAppAPI.DTO;

/// <summary>
/// Root response from the WhatsApp Business API when retrieving message templates.
/// </summary>
public class WhatsAppTemplateResponse
{
    [JsonPropertyName("data")]
    public IList<WhatsAppTemplate> Data { get; set; } = [];

    [JsonPropertyName("paging")]
    public PagingInfo? Paging { get; set; }
}

/// <summary>
/// Represents a single WhatsApp message template.
/// </summary>
public class WhatsAppTemplate
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("parameter_format")]
    public string ParameterFormat { get; set; } = string.Empty;

    [JsonPropertyName("components")]
    public IList<TemplateComponent> Components { get; set; } = [];

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("sub_category")]
    public string? SubCategory { get; set; }

    [JsonPropertyName("disable_ios_autofill")]
    public bool DisableIosAutofill { get; set; }

    [JsonPropertyName("is_primary_device_delivery_only")]
    public bool IsPrimaryDeviceDeliveryOnly { get; set; }

    [JsonPropertyName("library_template_name")]
    public string? LibraryTemplateName { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// A component within a WhatsApp template (HEADER, BODY, FOOTER, etc.).
/// </summary>
public class TemplateComponent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("example")]
    public TemplateExample? Example { get; set; }
}

/// <summary>
/// Example payload for a template component, containing either named or positional parameters.
/// </summary>
public class TemplateExample
{
    [JsonPropertyName("body_text_named_params")]
    public IList<NamedParam>? BodyTextNamedParams { get; set; }

    [JsonPropertyName("body_text")]
    public IList<IList<string>>? BodyText { get; set; }
}

/// <summary>
/// A single named parameter example within a template body.
/// </summary>
public class NamedParam
{
    [JsonPropertyName("param_name")]
    public string ParamName { get; set; } = string.Empty;

    [JsonPropertyName("example")]
    public string Example { get; set; } = string.Empty;
}

/// <summary>
/// Paging information for paginated template responses.
/// </summary>
public class PagingInfo
{
    [JsonPropertyName("cursors")]
    public CursorInfo? Cursors { get; set; }
}

/// <summary>
/// Cursor pointers for navigating paginated results.
/// </summary>
public class CursorInfo
{
    [JsonPropertyName("before")]
    public string? Before { get; set; }

    [JsonPropertyName("after")]
    public string? After { get; set; }
}
