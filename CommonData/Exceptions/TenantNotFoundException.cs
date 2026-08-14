namespace CommonData.Exceptions;

public class TenantNotFoundException : AppException
{
    public TenantNotFoundException(string tenantId)
        : base($"Tenant '{tenantId}' not found." ,
               AppErrorType.NotFound ,
               code: "TENANT_NOT_FOUND" ,
               details: new Dictionary<string , object?> { ["tenantId"] = tenantId })
    {
    }
}
