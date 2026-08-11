using System.Collections.Generic;

namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// A component within a template (HEADER, BODY, FOOTER, BUTTONS).
/// </summary>
public class TemplateComponentDTO
{
    public string Type { get; set; } = string.Empty;

    public string? Format { get; set; }

    public string? Text { get; set; }

    public TemplateExampleDTO? Example { get; set; }

    public IList<TemplateButtonDTO>? Buttons { get; set; }
}
