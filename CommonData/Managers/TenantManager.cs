using CommonData.DAO;
using CommonData.VO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonData.Managers;

public class TenantManager
{
    private readonly IList<TenantVO> _tenantList = new List<TenantVO>();
    private readonly TenantDAO _dao = new();


    private static IHttpContextAccessor _httpContext = new HttpContextAccessor();

    /// <summary>
    /// Gets a tenant by ID. If not found in memory, fetches from DB.
    /// </summary>
    public async Task<TenantVO> GetTenantByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty." , nameof(token));

        // Search in cached list
        var tenant = _tenantList.FirstOrDefault(t => t.Token == token);
        if (tenant != null)
            return tenant;

        // Fallback: fetch from DB
        tenant = await _dao.GetByToken(token);
        if (tenant != null)
            _tenantList.Add(tenant);

        return tenant;
    }


    /// <summary>
    /// Returns all tenants currently cached in memory.
    /// </summary>
    public IReadOnlyList<TenantVO> GetAllCachedTenants() => _tenantList.AsReadOnly();

    /// <summary>
    /// Refreshes tenant list from DB.
    /// </summary>
    //public void RefreshTenants()
    //{
    //    _tenantList.Clear();
    //    _tenantList.AddRange(_tenantRepository.FetchAllTenants());
    //}
}

