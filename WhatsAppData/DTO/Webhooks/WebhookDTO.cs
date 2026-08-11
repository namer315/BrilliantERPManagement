using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.DTO.Webhooks;

public class WebhookDTO
{
    public string Object { get; set; } = string.Empty;

    public IList<EntryDTO> Entry { get; set; } = new List<EntryDTO>();
}
