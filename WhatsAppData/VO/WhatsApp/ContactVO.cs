using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppData.VO.WhatsApp;


[NHibernate.Envers.Configuration.Attributes.Audited]
public class ContactVO : EntityBaseWithCode
{
    public virtual string WhatsAppId { get; set; }   // WhatsApp unique ID
    public virtual string PhoneNumber { get; set; }
    public virtual string Name { get; set; }
}

public class ContactMap : EntityWithIdMapping<ContactVO>
{
    public ContactMap()
    {
        Map(x => x.PhoneNumber).Not.Nullable();
    }
}

