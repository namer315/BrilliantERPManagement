using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WhatsAppData.DTO.WhatsApp.Template;

public class TemplateSendDTO
{
    public string TemplateName { get; set; }
    public string RecipientPhoneNumber { get; set; }
    [JsonIgnore]
    public string LanguageCode { get; set; } = "en";
    public IList<TemplateParameterDTO> ParameterList { get; set; }
}
