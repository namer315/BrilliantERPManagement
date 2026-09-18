using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// Example payload for a component (named or positional body params, or header texts).
/// </summary>
public class TemplateExampleWDTO
{
    [JsonPropertyName("body_text_named_params")]
    public IList<NamedParamWDTO>? BodyTextNamedParams { get; set; }

    [JsonPropertyName("body_text")]
    public IList<IList<string>>? BodyText { get; set; }
}
