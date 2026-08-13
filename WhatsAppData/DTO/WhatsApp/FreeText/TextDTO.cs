using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.DTO.WhatsApp.FreeText;

public class TextDTO : FreeTextDTO
{

    public bool PreviewURL { get; set; } = true;
}
