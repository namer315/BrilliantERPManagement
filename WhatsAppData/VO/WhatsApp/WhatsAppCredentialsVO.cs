using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;

public class WhatsAppCredentialsVO : EntityBase
{
    public virtual string WABusinessAccountId { get; set; }
    public virtual string WAAccessToken { get; set; }
    //public string WAPhoneNumberId { get; set; }
}
public class WhatsAppCredentialsMap : EntityWithCreatedAtMapping<WhatsAppCredentialsVO>
{
    public WhatsAppCredentialsMap()
    {
        Map(x => x.WABusinessAccountId);
        Map(x => x.WAAccessToken);
        //Map(x => x.WAPhoneNumberId);
    }
}

