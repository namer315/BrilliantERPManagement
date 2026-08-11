using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp;

public class ContactDTO
{
    public string Input { get; set; }

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; }
}
