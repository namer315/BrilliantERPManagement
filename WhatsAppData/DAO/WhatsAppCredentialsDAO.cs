using CommonData.DAO;
using NHibernate;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppData.DAO;

public class WhatsAppCredentialsDAO : RepositoryBase
{
    /// <summary>
    /// Gets a single WhatsAppCredentialsVO by its Id.
    /// Returns null when no matching record is found.
    /// </summary>
    /// <param name="id">The Id of the WhatsAppCredentials record to retrieve.</param>
    public async Task<WhatsAppCredentialsVO> GetWhatsAppCredentialsById(Guid id)
    {
        IQuery q = Session.CreateQuery(@"
			FROM WhatsAppCredentialsVO as credentials
			WHERE
			    credentials.Id = :id
			")
            .SetParameter("id" , id)
            .SetMaxResults(1);

        return await q.UniqueResultAsync<WhatsAppCredentialsVO>();
    }

    /// <summary>
    /// Gets all WhatsAppCredentialsVO records.
    /// </summary>
    public async Task<IList<WhatsAppCredentialsVO>> GetAllWhatsAppCredentials()
    {
        IQuery q = Session.CreateQuery(@"
            FROM WhatsAppCredentialsVO as credentials
        ");

        return await q.ListAsync<WhatsAppCredentialsVO>();
    }

    /// <summary>
    /// Gets a single WhatsAppCredentialsVO by its WhatsApp Business Account ID.
    /// Returns null when no matching record is found.
    /// </summary>
    /// <param name="wABusinessAccountId">The WhatsApp Business Account ID to search for.</param>
    public async Task<WhatsAppCredentialsVO> GetWhatsAppCredentialsByBusinessAccountId(string wABusinessAccountId)
    {
        if (string.IsNullOrEmpty(wABusinessAccountId))
            return null;

        IQuery q = Session.CreateQuery(@"
			FROM WhatsAppCredentialsVO as credentials
			WHERE
			    credentials.WABusinessAccountId = :wABusinessAccountId
			")
            .SetParameter("wABusinessAccountId" , wABusinessAccountId)
            .SetMaxResults(1);

        return await q.UniqueResultAsync<WhatsAppCredentialsVO>();
    }
}
