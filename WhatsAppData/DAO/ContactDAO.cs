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
    public async Task<ContactVO> GetContactBy(string phoneNumber)
    {
        IQuery q = Session.CreateQuery(@"
			FROM ContactVO as contact
			WHERE
			    contact.PhoneNumber = :phoneNumber
			")
             .SetParameter("phoneNumber" , phoneNumber)
             .SetMaxResults(1);

        return await q.UniqueResultAsync<ContactVO>();
    }
}
