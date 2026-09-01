using CommonData.VO;

namespace WhatsAppData.VO.WhatsApp;


[NHibernate.Envers.Configuration.Attributes.Audited]
public class ContactVO : EntityBaseWithCode
{
    //public virtual string WhatsAppId { get; set; }   // WhatsApp unique ID
    public virtual string Phone { get; set; }
    public virtual string WaId { get; set; }
    public virtual string PhoneNumberId { get; set; }
    public virtual string Name { get; set; }

    public virtual WhatsAppTenantVO WhatsAppTenant { get; set; }
}

public class ContactMap : EntityBaseCodeWithIdMapping<ContactVO>
{
    public ContactMap()
    {
        Map(x => x.Phone).Nullable();
        Map(x => x.WaId).Not.Nullable();
        Map(x => x.Name).Nullable();
        Map(x => x.PhoneNumberId).Nullable();


        References(x => x.WhatsAppTenant, "WhatsAppTenant").Cascade.None().Nullable();
    }
}

