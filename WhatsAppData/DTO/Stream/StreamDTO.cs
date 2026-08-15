using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.Stream;

public class StreamDTO
{
#if !DEBUG
    [JsonIgnore]
#endif
    public string Token { get; set; }

    public string TenentName { get; set; }

    public StreamMessageDTO Message { get; set; }
}
