using CommonData.VO;

namespace WhatsAppData.VO.WhatsApp;

public class WhatsAppTenantVO : EntityBase
{
    //    public string WABusinessAccountId { get; set; }
    //    public string WAAccessToken { get; set; }
    public virtual string WAPhoneNumberId { get; set; }

    public virtual WhatsAppCredentialsVO WhatsAppCredentials { get; set; }
    public virtual ContactVO Contact { get; set; }
    public virtual TenantVO Tenant { get; set; }
}

public class WhatsAppTenantMap : EntityWithCreatedAtMapping<WhatsAppTenantVO>
{
    public WhatsAppTenantMap()
    {
        //Map(x => x.WABusinessAccountId);
        //Map(x => x.WAAccessToken);
        Map(x => x.WAPhoneNumberId);


        References(x => x.Contact, "Contact").Cascade.Merge();
        References(x => x.Tenant , "Tenant").Cascade.Merge();
        References(x => x.WhatsAppCredentials , "WhatsAppCredentials").Cascade.Merge();
    }
}
