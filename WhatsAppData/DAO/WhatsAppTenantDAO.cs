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
                tenant.Name, 
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
            FROM WhatsAppTenantVO as tenant
            WHERE tenant.Id = :id
        ")
        .SetParameter("id" , id)
        .SetMaxResults(1);

        return await q.UniqueResultAsync<WhatsAppTenantVO>();
    }

    public async Task<IList<WhatsAppTenantVO>> GetAllActiveAsync()
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppTenantVO as whatsappTenant
            WHERE whatsappTenant.Tenant.Active = true
        ");
        return await q.ListAsync<WhatsAppTenantVO>();
    }

    public async Task<IList<WhatsAppTenantVO>> GetAllInactiveAsync()
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppTenantVO as whatsappTenant
            WHERE whatsappTenant.Tenant.Active = false
        ");
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
