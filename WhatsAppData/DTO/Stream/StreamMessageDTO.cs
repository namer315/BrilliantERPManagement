using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.DTO.Stream;

public class StreamMessageDTO
{
    public string From { get; set; }
    public string Content { get; set; }
    public DateTimeOffset DateTimeUTC { get; set; }
}
