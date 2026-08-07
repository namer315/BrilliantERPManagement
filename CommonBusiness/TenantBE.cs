using CommonData.DAO;
using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonBusiness;

public class TenantBE
{
    TenantDAO _dao = new TenantDAO();

    /// <summary>
    /// Persists a new tenant or updates an existing one.
    /// </summary>
    //public string PersistAsync(TenantVO tenant)
    //{

    //    return _dao.PersistAsync(tenant);
    //}

    public TenantVO TenantVO { get; set; }

    public TenantVO ResolveTenantByToken(string token)
    {
        TenantVO tenant = null;
        // Implementation for resolving tenant by token
        return tenant;
    }
}
