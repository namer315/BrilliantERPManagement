using CommonData.DAO;
using NHibernate;
using System.Collections;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DAO;

public class MessageStatusDAO : RepositoryBase
{
    /*public async Task<IList<(Guid MessageId , MessageStatusVO.WhatsAppMessageStatus Status)>> GetMessageStatusBy(IList<Guid> messageIdList)
    {
        var results = await Session.CreateQuery(@"
        SELECT status.Message.Id, status.Status
        FROM MessageStatusVO as status
        WHERE status.Message.Id IN (:messageIds)
          AND status.Timestamp = (
              SELECT MAX(s.Timestamp)
              FROM MessageStatusVO as s
              WHERE s.Message.Id = status.Message.Id
          )
    ")
        .SetParameterList("messageIds" , messageIdList)
        .ListAsync<object[]>();

        // Map results into a tuple list
        return results.Select(r => ((Guid)r[0] , (MessageStatusVO.WhatsAppMessageStatus)r[1]))
                      .ToList();
    }*/

    public async Task<IList<(Guid Id , MessageStatusVO.WhatsAppMessageStatus Status)>> GetMessageStatusBy(IList<Guid> messageIdList)
    {
        /*IQuery q = Session.CreateQuery(@"
            SELECT message.Id, MAX(status.Status)
            FROM MessageStatusVO as status
                LEFT JOIN status.Message as message
            WHERE message.Id IN (:messageIdList)
            GROUP BY message.Id
            ")*/
        IQuery q = Session.CreateQuery(@"
            SELECT status.Message.Id, status.Status
            FROM MessageStatusVO as status
            WHERE status.Message.Id IN (:messageIdList)
              AND status.Timestamp = (
                  SELECT MAX(s.Timestamp)
                  FROM MessageStatusVO as s
                  WHERE s.Message.Id = status.Message.Id
              )"
        )
            .SetParameterList("messageIdList" , messageIdList);

        var results = await q.ListAsync<object[]>();
        return results.Select(r => ((Guid)r[0] , (MessageStatusVO.WhatsAppMessageStatus)r[1]))
                      .ToList();
    }


}
