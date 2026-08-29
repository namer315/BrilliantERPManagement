using CommonData.VO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.VO;

public class WhatsAppTenantVO : EntityBase
{
    public string WABusinessAccountId { get; set; }
    public string WAAccessToken { get; set; }
    public string WAPhoneNumberId { get; set; }

    public virtual ContactVO Contact { get; set; }
    public virtual TenantVO Tenant { get; set; }
}

public class WhatsAppTenantMap : EntityWithCreatedAtMapping<WhatsAppTenantVO>
{
    public WhatsAppTenantMap()
    {
        Map(x => x.WABusinessAccountId);
        Map(x => x.WAAccessToken);
        Map(x => x.WAPhoneNumberId);


        References(x => x.Contact, "Contact").Cascade.Merge();
        References(x => x.Tenant, "Tenant").Cascade.Merge();
    }
}
