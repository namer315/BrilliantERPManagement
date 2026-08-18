using CommonData.DAO;
using CommonData.VO;
using NHibernate;
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

    /// <summary>
    /// Gets the last (most recent) message whose Sender matches the given number,
    /// searching the sender's PhoneNumber OR its WhatsApp WaId.
    /// Returns null when no matching message is found.
    /// </summary>
    /// <param name="number">The sender's phone number or WaId to search for.</param>
    /// <exception cref="ArgumentException">Thrown when number is null or empty.</exception>
    public async Task<MessageVO> GetLastMessageBySender(string number)
    {
        //SELECT 
        //    message.Id, message.CreatedAt, message.Content, message.ReceivedAt, message.Status, message.Type, message.Timestamp
        //    , message.Sender, message.Receiver, message.Tenant
        IQuery q = Session.CreateQuery(@"
            FROM MessageVO as message
                LEFT OUTER JOIN FETCH message.Sender as sender
            WHERE
                sender IS NOT NULL 
                AND (sender.PhoneNumber = :number OR sender.WaId = :number)
            ORDER BY message.Id DESC"
        )
            .SetParameter("number" , number)
            .SetMaxResults(1);

        return await q.UniqueResultAsync<MessageVO>();
    }

    public async Task<IList<TenantVO>> GetTenantsByContact(ContactVO sender)
    {
        var results = await Session.CreateQuery(@"
            SELECT DISTINCT message.Tenant, message.CreatedAt
            FROM MessageVO as message
                LEFT OUTER JOIN message.Receiver as receiver
            WHERE
                receiver IS NOT NULL 
                AND
                receiver.Id = :receiverId
            ORDER BY message.CreatedAt DESC"
        )
            .SetParameter("receiverId" , sender.Id)
            .ListAsync<object[]>();

        // Extract only the TenantVO from the object array tuples
        return results.Select(r => (TenantVO)r[0]).ToList();
    }

    public async Task<IList<MessageVO>> GetMessageHistoryBy(string waId)
    {
        IQuery q = Session.CreateQuery(@"
            FROM MessageVO as message
                LEFT OUTER JOIN FETCH message.Sender as sender
                LEFT OUTER JOIN FETCH message.Receiver as receiver
            WHERE
                (sender.WaId = :waId OR receiver.WaId = :waId)
            ORDER BY message.CreatedAt DESC"
        )
            .SetParameter("waId" , waId);

        return await q.ListAsync<MessageVO>();
    }
}
