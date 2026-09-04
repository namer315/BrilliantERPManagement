using CommonData.DAO;
using CommonData.Managers;
using CommonData.VO;
using NHibernate;
using WhatsAppData.Search.Chat;
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
    public async Task<IList<MessageVO>> GetMessageHistoryBy(Guid contactId , ChatHistorySH chatHistorySH)
    {
        string whereCondition = string.Empty;
        if (!string.IsNullOrEmpty(chatHistorySH.MessageId))
            whereCondition += @" AND message.CreatedAt < (
                SELECT ref.CreatedAt
                FROM MessageVO ref
                WHERE ref.MessageId = :messageId
            )";
        IQuery q = Session.CreateQuery($@"
        FROM MessageVO as message
            LEFT OUTER JOIN FETCH message.Sender as sender
            LEFT OUTER JOIN FETCH message.Receiver as receiver
            LEFT OUTER JOIN FETCH message.Tenant as tenent
        WHERE
            (tenent IS NOT NULL AND tenent.Id = :tenentId)            
            AND (sender.Id = :contactId OR receiver.Id = :contactId)
            {whereCondition}
        ORDER BY message.CreatedAt DESC"
        )
        .SetParameter("contactId" , contactId)
        .SetParameter("tenentId" , TenantManager.CurrentTenant.Id)
        .SetFirstResult(chatHistorySH.Offset)       // use model property
        .SetMaxResults(chatHistorySH.PageSize);     // use model property

        if (!string.IsNullOrEmpty(chatHistorySH.MessageId))
            q.SetParameter("messageId" , chatHistorySH.MessageId);

        return await q.ListAsync<MessageVO>();
    }

    /*public async Task<IList<MessageVO>> GetMessageHistoryBy(Guid id , int pageNumber , int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;
            //throw new ArgumentOutOfRangeException(nameof(pageNumber) , "Page number must be greater than 0.");
        if (pageSize < 1)
            pageSize = 30;
            //throw new ArgumentOutOfRangeException(nameof(pageSize) , "Page size must be greater than 0.");

        IQuery q = Session.CreateQuery(@"
        FROM MessageVO as message
            LEFT OUTER JOIN FETCH message.Sender as sender
            LEFT OUTER JOIN FETCH message.Receiver as receiver
        WHERE
            (sender.Id = :id OR receiver.Id = :id)
        ORDER BY message.CreatedAt DESC"
        )
        .SetParameter("id" , id)
        .SetFirstResult((pageNumber - 1) * pageSize)   // offset
        .SetMaxResults(pageSize);                      // limit

        return await q.ListAsync<MessageVO>();
    }*/


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

    /*public IList<LatestMessageDto> GetLatestMessagesForContacts(IList<long> contactIds)
    {
        if (contactIds == null || contactIds.Count == 0)
            return new List<LatestMessageDto>();

        // HQL query using correlated subquery to fetch the record with the maximum CreatedAt per conversation/contact
        string hql = @"
        select 
            m.MessageId as MessageId,
            m.Content as Content,
            m.CreatedAt as CreatedAt,
            m.Sender as Sender,
            m.Receiver as Receiver
        from MessageVO m
        where (m.Sender.Id in (:contactIds) or m.Receiver.Id in (:contactIds))
          and m.CreatedAt = (
              select max(sub.CreatedAt) 
              from MessageVO sub 
              where (sub.Sender.Id = m.Sender.Id and sub.Receiver.Id = m.Receiver.Id)
                 or (sub.Sender.Id = m.Receiver.Id and sub.Receiver.Id = m.Sender.Id)
          )";

        return Session.CreateQuery(hql)
            .SetParameterList("contactIds" , contactIds)
            .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean<LatestMessageDto>())
            .List<LatestMessageDto>();
    }*/

    /*public async Task<IList> GetLatestMessagesForContacts(IList<Guid> contactIdList)
    {
        IQuery q = Session.CreateQuery(@"
        SELECT
            sender.Id,
            receiver.Id,
            m.Id,
            m.Content as Content,
            m.CreatedAt as Timestamp,
            m.MessageId as MessageId
        FROM MessageVO m
            LEFT OUTER JOIN m.Sender as sender
            LEFT OUTER JOIN m.Receiver as receiver
        WHERE
            (m.Sender.Id in (:contactIdList) or m.Receiver.Id in (:contactIdList))
            and m.CreatedAt = (
                select max(sub.CreatedAt)
                from MessageVO sub
                where (sub.Sender.Id = m.Sender.Id and sub.Receiver.Id = m.Receiver.Id)
                    or (sub.Sender.Id = m.Receiver.Id and sub.Receiver.Id = m.Sender.Id)
            )"
    )
        .SetParameterList("contactIdList" , contactIdList);

        return await q.ListAsync();
    }*/
    public async Task<IList<MessageVO>> GetLatestMessagesForContacts(IList<Guid> contactIdList)
    {
        if (contactIdList == null || !contactIdList.Any())
            return new List<MessageVO>();

        IQuery q = Session.CreateQuery(@"
                FROM MessageVO m
                WHERE (m.Sender.Id IN (:contactIdList) OR m.Receiver.Id IN (:contactIdList))
                  AND m.Timestamp = (
                      SELECT MAX(sub.Timestamp)
                      FROM MessageVO sub
                      WHERE
                         (sub.Sender.Id = m.Sender.Id AND sub.Receiver.Id = m.Receiver.Id)
                          OR
                          (sub.Sender.Id = m.Receiver.Id AND sub.Receiver.Id = m.Sender.Id)
                  )
            ")
            .SetParameterList("contactIdList" , contactIdList);
        return await q.ListAsync<MessageVO>();
    }
}
