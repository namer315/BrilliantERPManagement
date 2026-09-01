using CommonData.DAO;
using CommonData.VO;
using NHibernate;

namespace CommonData.Extensions;

public static class EntityBaseExtension
{
    public static async Task<T> GetLastCodeNumber<T>(
        this RepositoryBase repository ,
        string whereCondition = "")
        where T : EntityBaseWithCode, new()
    {
        string entityName = typeof(T).Name.Replace("Proxy" , "");

        if (!string.IsNullOrWhiteSpace(whereCondition))
            whereCondition += " AND " + whereCondition;

        IQuery q = RepositoryBase.Session.CreateQuery($@"
            SELECT e.Code, e.Number
            FROM {entityName} e
            WHERE e.Number = (
                SELECT MAX(x.Number)
                FROM {entityName} x
            )")
            .SetMaxResults(1);

        //var entity = await q.UniqueResultAsync<T>();
        object[] rawData = await q.UniqueResultAsync<object[]>();

        T entity = new T();
        if (rawData is { Length: > 0 } && rawData[0] is not null)
        {
            entity.Code = Convert.ToString(rawData[0]);
            entity.Number = Convert.ToInt64(rawData[1]);
        }
        else
        {
            entity.Code = "";
            entity.Number = 0;
        }
        // Return a new instance if nothing found
        return entity;
    }

    public static async Task<bool> IsCodeExists<T>(
        this RepositoryBase repository ,
        string code ,
        Guid id ,
        string whereCondition = "" ,
        string leftJoin = "")
        where T : EntityBaseWithCode
    {
        string entityName = typeof(T).Name.Replace("Proxy" , "");

        IQuery q = RepositoryBase.Session.CreateQuery($@"
                SELECT 1
                FROM {entityName} as entity
                {leftJoin}
                WHERE entity.Code = :code
                  AND entity.Id <> :id
                  {whereCondition}
            ")
            .SetParameter("code" , code)
            .SetParameter("id" , id)
            .SetReadOnly(true)
            .SetMaxResults(1);

        var result = await q.UniqueResultAsync<int?>();
        return result.HasValue;
    }

    public static async Task<bool> IsNumberExists<T>(
        this RepositoryBase repository ,
        long number ,
        Guid id ,
        string whereCondition = "" ,
        string leftJoin = "")
        where T : EntityBaseWithCode
    {
        string entityName = typeof(T).Name.Replace("Proxy" , "");

        IQuery q = RepositoryBase.Session.CreateQuery($@"
            SELECT 1
            FROM {entityName} as entity
            {leftJoin}
            WHERE entity.Number = :number
              AND entity.Id <> :id
              {whereCondition}
        ")
            .SetParameter("number" , number)
            .SetParameter("id" , id)
            .SetReadOnly(true)
            .SetMaxResults(1);

        var result = await q.UniqueResultAsync<int?>();
        return result.HasValue;

    }
}
