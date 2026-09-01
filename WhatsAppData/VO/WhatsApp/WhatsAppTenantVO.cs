using CommonData.VO;

namespace WhatsAppData.VO.WhatsApp;

public class WhatsAppTenantVO : EntityBaseWithCode
{
    //    public string WABusinessAccountId { get; set; }
    //    public string WAAccessToken { get; set; }
    //public virtual string WAPhoneNumberId { get; set; }

    public virtual WhatsAppCredentialsVO WhatsAppCredentials { get; set; } = new WhatsAppCredentialsVO();
    public virtual ContactVO Contact { get; set; } = new ContactVO();
    public virtual TenantVO Tenant { get; set; } = new TenantVO();

    public virtual bool IsPublicNumber { get; set; }
}

public class WhatsAppTenantMap : EntityCodeWithCreatedAtMapping<WhatsAppTenantVO>
{
    public WhatsAppTenantMap()
    {
        //Map(x => x.WABusinessAccountId);
        //Map(x => x.WAAccessToken);
        //Map(x => x.WAPhoneNumberId);


        References(x => x.Contact, "Contact").Cascade.Merge();
        References(x => x.Tenant , "Tenant").Cascade.Merge();
        References(x => x.WhatsAppCredentials , "WhatsAppCredentials").Cascade.Merge();
    }
}
