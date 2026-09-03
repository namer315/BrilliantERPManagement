using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using WhatsAppData.DTO.Webhooks;
using static WhatsAppData.VO.WhatsApp.MessageVO;

namespace WhatsAppData.DTO.FreeText;

public class FreeTextDTO
{
    public WhatsAppMessageTypes MessageType { get; set; }
    public string Phone { get; set; }
    public string Body { get; set; }

    public FreeTextTextDTO Text { get; set; }
    public FreeTextImageDTO Image { get; set; }
    public FreeTextVideoDTO Video { get; set; }
    public FreeTextAudioDTO Audio { get; set; }
    public FreeTextDocumentDTO Document { get; set; }
}
public class FreeTextTextDTO
{
    public bool PreviewURL { get; set; } = true;
}
public class FreeTextImageDTO
{
    [JsonIgnore]
    public string Id { get; set; }
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    //public string Caption { get; set; }
}
public class FreeTextVideoDTO
{
    [JsonIgnore]
    public string Id { get; set; }
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    //public string Caption { get; set; }
}
public class FreeTextAudioDTO
{
    [JsonIgnore]
    public string Id { get; set; }
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
}
public class FreeTextDocumentDTO
{
    [JsonIgnore]
    public string Id { get; set; }
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public string Caption { get; set; }
}