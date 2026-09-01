using FluentNHibernate.Mapping;

namespace CommonData.VO;

/// <summary>
/// Tenant entity representing a multi-tenant ERP subscriber.
/// </summary>
[NHibernate.Envers.Configuration.Attributes.Audited]
public class TenantVO : EntityBaseWithCode
{
    public virtual string Name { get; set; } = string.Empty;
    public virtual string Token { get; set; } = string.Empty;
    public virtual bool Active { get; set; }
}

/// <summary>
/// Embedded Fluent NHibernate mapping for TenantVO.
/// </summary>
public class TenantMap : EntityBaseCodeWithIdMapping<TenantVO>
{
    public TenantMap()
    {
        Map(x => x.Name)
            .Not.Nullable()
            .Length(200)
            .UniqueKey("UK_ERP_Tenant_Name");

        Map(x => x.Token)
            .Not.Nullable()
            .Length(500)
            .UniqueKey("UK_ERP_Tenant_Token");

        Map(x => x.Active)
            .Not.Nullable()
            .Default("1");
    }
}
