using CommonData.VO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.VO;

public class WhatsAppTenantVO : EntityBase
{
    public virtual ContactVO Contact { get; set; }
    public virtual TenantVO Tenant { get; set; }
}

//public class TenantMap : EntityWithCreatedAtMapping<WhatsAppTenantVO>
//{
//    public TenantMap()
//    {
//        References(x => x.Contact).Cascade.Merge();
//    }
//}
