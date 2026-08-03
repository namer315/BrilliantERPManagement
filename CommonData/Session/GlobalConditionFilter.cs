using NHibernate;
using NHibernate.Engine;

namespace CommonData.Session;

/// <summary>
/// Defines and manages NHibernate global filters for:
///   - SoftDeleteFilter: excludes IsDeleted=true rows
///   - TenantFilter: restricts rows to the current tenant (ITenantAware entities)
///
/// Filters are enabled on the ISession by NHibernateUnitOfWork.
/// </summary>
public static class GlobalConditionFilter
{
    public const string SoftDeleteFilterName = "SoftDeleteFilter";
    public const string TenantFilterName = "TenantFilter";

    /// <summary>
    /// Registers filter definitions into the NHibernate Configuration.
    /// Call during SessionFactoryManager initialization.
    /// </summary>
    public static void RegisterFilterDefinitions(NHibernate.Cfg.Configuration cfg)
    {
        cfg.AddFilterDefinition(new NHibernate.Engine.FilterDefinition(
            SoftDeleteFilterName,
            defaultCondition: ":IsDeleted = 0",
            parameterTypes: new Dictionary<string, NHibernate.Type.IType>(),
            useManyToOne: false));

        cfg.AddFilterDefinition(new NHibernate.Engine.FilterDefinition(
            TenantFilterName,
            defaultCondition: ":TenantId = TenantId",
            parameterTypes: new Dictionary<string, NHibernate.Type.IType>
            {
                { "TenantId", NHibernateUtil.Int64 }
            },
            useManyToOne: false));
    }

    /// <summary>
    /// Enables the soft-delete filter on the given session.
    /// </summary>
    public static void EnableSoftDelete(ISession session)
    {
        session.EnableFilter(SoftDeleteFilterName);
    }

    /// <summary>
    /// Enables the tenant isolation filter for the given tenant.
    /// No-op if tenant is null (no filtering applied).
    /// </summary>
    public static void EnableTenantFilter(ISession session, long tenantId)
    {
        session.EnableFilter(TenantFilterName)
               .SetParameter("TenantId", tenantId);
    }
}
