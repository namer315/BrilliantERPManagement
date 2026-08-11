using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp;

public class MessageDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("message_status")]
    public string MessageStatus { get; set; }
}
