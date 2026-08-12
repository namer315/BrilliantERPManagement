using CommonData.DAO;
using Google.Protobuf;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DAO;

public class MessageDAO : RepositoryBase
{
    public async Task<MessageVO> GetMessageBy(string messageId)
    {
        IQuery q = Session.CreateQuery(@"
			FROM MessageVO as message
			WHERE
			    message.MessageId = :messageId
			")
            .SetParameter("messageId" , messageId)
            .SetMaxResults(1);

        return await q.UniqueResultAsync<MessageVO>();
    }
}
