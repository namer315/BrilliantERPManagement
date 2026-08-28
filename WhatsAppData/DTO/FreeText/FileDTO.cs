using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.DTO.FreeText;

public class FileDTO : MessageTextDTO
{
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
}