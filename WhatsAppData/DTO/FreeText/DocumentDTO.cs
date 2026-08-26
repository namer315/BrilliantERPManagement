using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using WhatsAppData.DTO.WhatsApp.FreeText;

namespace WhatsAppData.DTO.FreeText;

public class DocumentDTO : MessageTextDTO
{
    //[JsonIgnore]
    //public string Id { get; set; }
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    //public bool PreviewURL { get; set; } = true;
}