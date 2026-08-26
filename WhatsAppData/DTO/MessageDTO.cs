using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.DTO;

public class MessageDTO
{
    public string Phone { get; set; }
}

public class MessageTextDTO : MessageDTO
{
    public string Body { get; set; }
}
