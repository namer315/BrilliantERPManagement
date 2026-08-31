using CommonData.DAO;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhatsAppData.VO;

namespace WhatsAppData.DAO;

public class WhatsAppTenantDAO : RepositoryBase
{
     public async Task<IList> GetAllAsync()
    {
        IQuery q = Session.CreateQuery(@"
            SELECT whatsappTenant.Id, whatsappTenant.WABusinessAccountId, whatsappTenant.WAPhoneNumberId, tenant.Name, contact.Name, contact.PhoneNumber
            FROM WhatsAppTenantVO as whatsappTenant
            LEFT JOIN whatsappTenant.Tenant as tenant
            LEFT JOIN whatsappTenant.Contact as contact
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
}
