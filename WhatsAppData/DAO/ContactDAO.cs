using CommonData.DAO;
using CommonData.VO;
using MySqlX.XDevAPI;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DAO;

public class ContactDAO : RepositoryBase
{
    public async Task<ContactVO> GetContactBy(string waId)
    {
        IQuery q = Session.CreateQuery(@"
			FROM ContactVO as contact
			WHERE
			    contact.WaId = :waId
			")
             .SetParameter("waId" , waId)
             .SetMaxResults(1);

        return await q.UniqueResultAsync<ContactVO>();
    }
}
