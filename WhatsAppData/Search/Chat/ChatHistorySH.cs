using CommonData.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.Search.Chat;

public class ChatHistorySH : Pagination
{
    public string WaId { get; set; }
    public string MessageId { get; set; }
}
