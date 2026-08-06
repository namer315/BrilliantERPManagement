using FluentNHibernate.Mapping;
using NHibernate;

namespace CommonData.Session;

/// <summary>
/// Defines and manages NHibernate global filters for:
///   - SoftDeleteFilter: excludes IsDeleted=true rows
///   - TenantFilter: restricts rows to the current tenant (ITenantAware entities)
///
/// Filters are enabled on the ISession by NHibernateUnitOfWork.
/// </summary>
public class GlobalConditionFilter : FilterDefinition
{
    public GlobalConditionFilter()
    {
        WithName("GlobalFilter").AddParameter("name" , NHibernate.NHibernateUtil.String);
    }
}

