using CommonData.DAO;
using NHibernate;
using System.Collections;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DAO;

public class WhatsAppTenantDAO : RepositoryBase
{
    public async Task<IList> GetAllAsync()
    {
        IQuery q = Session.CreateQuery(@"
            SELECT 
                whatsappTenant.Id,
                whatsapp.WABusinessAccountId,
                tenant.Name, tenant.Active,
                contact.WaId , contact.PhoneNumberId
            FROM WhatsAppTenantVO as whatsappTenant
            LEFT JOIN whatsappTenant.Tenant as tenant
            LEFT JOIN whatsappTenant.Contact as contact
            LEFT JOIN whatsappTenant.WhatsAppCredentials as whatsapp
        ");
        return await q.ListAsync();
    }

    public async Task<WhatsAppTenantVO> GetByIdAsync(Guid id)
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppTenantVO as whatsAppTenant
            WHERE whatsAppTenant.Id = :id
        ")
        .SetParameter("id" , id)
        .SetMaxResults(1);

        return await q.UniqueResultAsync<WhatsAppTenantVO>();
    }

    /// <summary>
    /// Retrieves the WhatsApp tenant associated with the given ERP tenant id.
    /// Returns the full model including the nested <see cref="WhatsAppCredentialsVO"/>,
    /// <see cref="ContactVO"/> and <see cref="TenantVO"/> references.
    /// </summary>
    public async Task<WhatsAppTenantVO> GetByTenantIdAsync(Guid tenantId)
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppTenantVO as whatsappTenant
            WHERE whatsappTenant.Tenant.Id = :tenantId
        ")
        .SetParameter("tenantId" , tenantId)
        .SetMaxResults(1);

        return await q.UniqueResultAsync<WhatsAppTenantVO>();
    }

    public async Task<IList<WhatsAppTenantVO>> GetByActiveAsync(bool active)
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppTenantVO as whatsappTenant
            WHERE whatsappTenant.Tenant.Active = :active
        ")
        .SetParameter("active" , active);
        return await q.ListAsync<WhatsAppTenantVO>();
    }

    public async Task<WhatsAppTenantVO> GetByIdIfActiveAsync(Guid id)
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppTenantVO as whatsappTenant
            WHERE whatsappTenant.Id = :id
            AND whatsappTenant.Tenant.Active = true
        ")
        .SetParameter("id" , id)
        .SetMaxResults(1);

        return await q.UniqueResultAsync<WhatsAppTenantVO>();
    }
}
