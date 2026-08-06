using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonData.DAO.FNHConfig;

internal class GlobalConditionFilter : FilterDefinition
{
    public GlobalConditionFilter()
    {
        WithName("GlobalFilter").AddParameter("name" , NHibernate.NHibernateUtil.String);
    }
}
