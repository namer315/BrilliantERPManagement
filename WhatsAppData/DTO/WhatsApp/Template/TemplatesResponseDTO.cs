using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp.Template;

/// <summary>
/// Root response from GET {WABA_ID}/message_templates.
/// </summary>
public class TemplatesResponseDTO
{
    public IList<MessageTemplateDTO> Data { get; set; }

    public PagingDTO? Paging { get; set; }
}

