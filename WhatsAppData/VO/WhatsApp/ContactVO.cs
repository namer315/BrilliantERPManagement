using CommonData.VO;

namespace WhatsAppData.VO.WhatsApp;


[NHibernate.Envers.Configuration.Attributes.Audited]
public class ContactVO : EntityBaseWithCode
{
    //public virtual string WhatsAppId { get; set; }   // WhatsApp unique ID
    public virtual string PhoneNumber { get; set; }
    public virtual string WaId { get; set; }
    public virtual string PhoneNumberId { get; set; }
    public virtual string Name { get; set; }

    public virtual WhatsAppTenantVO WhatsAppTenant { get; set; }
}

public class ContactMap : EntityWithIdMapping<ContactVO>
{
    public ContactMap()
    {
        Map(x => x.PhoneNumber).Nullable();
        Map(x => x.WaId).Not.Nullable();
        //Map(x => x.Name).Not.Nullable();


        References(x => x.WhatsAppTenant, "WhatsAppTenant").Cascade.Merge();
    }
}

