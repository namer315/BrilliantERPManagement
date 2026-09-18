using System.Collections.Generic;

namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// A component within a template (HEADER, BODY, FOOTER, BUTTONS).
/// </summary>
public class TemplateComponentWDTO
{
    public string Type { get; set; } = string.Empty;

    public string? Format { get; set; }

    public string? Text { get; set; }

    public TemplateExampleWDTO? Example { get; set; }

    public IList<TemplateButtonWDTO>? Buttons { get; set; }
}
