using CommonData.DAO;
using CommonData.Managers;
using CommonData.VO;
using MySqlX.XDevAPI;
using NHibernate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DAO;

public class ContactDAO : RepositoryBase
{
    public async Task<ContactVO> GetContactBy(string waId)
    {
        if (string.IsNullOrEmpty(waId))
            return null;

        IQuery q = Session.CreateQuery(@"
			FROM ContactVO as contact
			WHERE
			    contact.WaId = :waId
			")
             .SetParameter("waId" , waId)
             .SetMaxResults(1);

        return await q.UniqueResultAsync<ContactVO>();
    }

    public async Task<IList<ContactVO>> GetChatListContacts()
    {
        IQuery query = Session.CreateQuery(@"
        SELECT DISTINCT contact
        FROM ContactVO AS contact
        WHERE EXISTS (
            FROM MessageVO AS message
            WHERE message.Tenant.Id = :tenantId
              AND (message.Sender = contact OR message.Receiver = contact)
        )
        ORDER BY contact.WaId
    ")
    .SetParameter("tenantId" , TenantManager.CurrentTenant.Id);

        return await query.ListAsync<ContactVO>();
    }

    public async Task<ContactVO> GetContactByPhoneNumberId(string phoneNumberId)
    {
        IQuery q = Session.CreateQuery(@"
			FROM ContactVO as contact
			WHERE
			    contact.PhoneNumberId = :phoneNumberId
			")
             .SetParameter("phoneNumberId" , phoneNumberId)
             .SetMaxResults(1);

        return await q.UniqueResultAsync<ContactVO>();
    }
}
