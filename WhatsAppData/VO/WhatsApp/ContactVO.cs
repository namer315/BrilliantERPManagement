using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class ContactVO : AppBaseEntityWithCode
{
    public virtual string WhatsAppId { get; set; }   // WhatsApp unique ID
    public virtual string PhoneNumber { get; set; }
    public virtual string Name { get; set; }
}

