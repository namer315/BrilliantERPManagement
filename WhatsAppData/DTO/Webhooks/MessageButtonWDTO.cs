using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Webhooks;

public class MessageButtonWDTO
{
    public string Payload { get; set; }

    public string Text { get; set; }
}
