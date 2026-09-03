using CommonData.DAO;
using CommonData.VO;
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

    public async Task<WhatsAppTenantVO> GetWhatsAppTenantBy(ContactVO contact)
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppTenantVO as whatsappTenant
                LEFT JOIN whatsappTenant.Tenant as tenant
                LEFT JOIN whatsappTenant.Contact as contact
            WHERE contact.Id = :contactId
        ")
        .SetParameter("contactId" , contact.Id)
        .SetMaxResults(1);

        return await q.UniqueResultAsync<WhatsAppTenantVO>();
    }
    public async Task<TenantVO> GetTenantBy(ContactVO contact)
    {
        IQuery q = Session.CreateQuery(@"
            SELECT tenant
            FROM WhatsAppTenantVO as whatsappTenant
                LEFT JOIN whatsappTenant.Tenant as tenant
                LEFT JOIN whatsappTenant.Contact as contact
            WHERE contact.Id = :contactId
        ")
        .SetParameter("contactId" , contact.Id)
        .SetMaxResults(1);

        return await q.UniqueResultAsync<TenantVO>();
    }
    public async Task<int> GetCountBy(ContactVO contact)
    {
        var count = await Session.CreateQuery(@"
            SELECT count(whatsappTenant)
            FROM WhatsAppTenantVO as whatsappTenant
                LEFT JOIN whatsappTenant.Contact as contact
            WHERE contact.Id = :contactId
        ")
            .SetParameter("contactId" , contact.Id)
            .UniqueResultAsync<long>();

        return (int)count;
    }
}
