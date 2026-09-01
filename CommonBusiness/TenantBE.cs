using CommonBusiness.Extensions;
using CommonData.DAO;
using CommonData.Managers;
using CommonData.VO;
using System.Security.Cryptography;

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
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("The authentication token is required and must not be empty. Please provide a valid API token to resolve the tenant.", nameof(token));

        // Implementation for resolving tenant by token
        TenantVO tenant =  await TenantManager.GetTenantByToken(token);

        // Handle the case where the tenant is not found
        if (tenant == null)
            throw new Exception("The specified tenant could not be located. Please verify your credentials and try again.");

        if (!tenant.Active) 
            throw new Exception($"The tenant {tenant.Name} is not active. Please contact the system administrator.");

        return tenant;
    }

    /// <summary>
    /// Generates a cryptographically random API token in the form "Brilliant-sk-&lt;base64&gt;".
    /// </summary>
    private string GenerateApiToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        string token = "Brilliant-sk-" + Convert.ToBase64String(bytes)
            .Replace("+" , "")
            .Replace("/" , "")
            .Replace("=" , "");

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("The API token generation process failed unexpectedly. The generated token is null or empty. Please retry the operation or contact the system administrator if the issue persists.");

        return token;
    }


    public void Validation(TenantVO tenant)
    {
        if (string.IsNullOrEmpty(tenant.Name))
            throw new ArgumentException("The tenant name is required and must not be empty. Please provide a valid name before registering a new tenant.", nameof(tenant.Name));
        if(_dao.IsNameExist(tenant))
            throw new InvalidOperationException($"A tenant with the name '{tenant.Name}' already exists in the system. Please choose a unique tenant name to avoid conflicts.");

        //ganarate token
        tenant.Token = GenerateApiToken();
        while (_dao.IsBrilliantTokenExist(tenant))
        {
            tenant.Token = GenerateApiToken();
        }
    }

    public async Task<TenantVO> GetNew()
    {
        TenantVO tenant = await _dao.GetNextCodeNumber<TenantVO>();

        return tenant;
    }
}
