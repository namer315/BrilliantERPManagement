using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// A single named parameter example within a template body.
/// </summary>
public class NamedParamDTO
{
    [JsonPropertyName("param_name")]
    public string ParamName { get; set; } = string.Empty;

    public string Example { get; set; } = string.Empty;
}
