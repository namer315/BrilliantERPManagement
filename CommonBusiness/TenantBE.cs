using CommonData.DAO;
using CommonData.Managers;
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

    public async Task<TenantVO> ResolveTenantByToken(string token)
    {
        // Implementation for resolving tenant by token
        TenantVO tenant =  await TenantManager.GetTenantByToken(token);

        // Handle the case where the tenant is not found
        if (tenant == null)
            throw new Exception("The specified tenant could not be located. Please verify your credentials and try again.");

        if (!tenant.Active) 
            throw new Exception($"The tenant {tenant.Name} is not active. Please contact the system administrator.");

        return tenant;
    }
}
