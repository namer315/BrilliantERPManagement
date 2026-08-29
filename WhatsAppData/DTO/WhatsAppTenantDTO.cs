using CommonData.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.DTO;

public class WhatsAppTenantDTO : TenantDTO
{
    public string WABusinessAccountId { get; set; }
    public string WAAccessToken { get; set; }
    public string WAPhoneNumberId { get; set; }
}
