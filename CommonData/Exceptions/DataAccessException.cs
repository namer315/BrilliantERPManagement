namespace CommonData.Exceptions;

/// <summary>
/// Wraps NHibernate-level exceptions into a DAL-consistent exception type.
/// DAO methods catch HibernateException/ADOException and rethrow as this.
/// </summary>
public class DataAccessException : Exception
{
    /// <summary>
    /// The entity type on which the operation was performed, if known.
    /// </summary>
    public string? EntityType { get; }

    /// <summary>
    /// The operation that failed (e.g., "GetById", "Save").
    /// </summary>
    public string? Operation { get; }

    public DataAccessException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public DataAccessException(string message, Exception inner, string entityType, string operation)
        : base(message, inner)
    {
        EntityType = entityType;
        Operation = operation;
    }

    public DataAccessException(string message)
        : base(message)
    {
    }
}
