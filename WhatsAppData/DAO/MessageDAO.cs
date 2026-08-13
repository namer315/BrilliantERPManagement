using CommonData.DAO;
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
}
