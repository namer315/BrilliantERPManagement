namespace CommonData.Exceptions;

/// <summary>
/// Base application exception. Each API project should derive its own exception
/// types from this class (or implement <see cref="IAppException"/>) so all
/// errors flow through one consistent handling path.
/// </summary>
public class AppException : Exception, IAppException
{
    /// <summary>Machine-readable error code (e.g. "TENANT_NOT_FOUND").</summary>
    public string? Code { get; }

    /// <summary>Category of the error.</summary>
    public AppErrorType Type { get; }

    /// <summary>HTTP status code returned to the caller.</summary>
    public int HttpStatusCode { get; }

    /// <summary>Optional structured details for the client / logs.</summary>
    public IReadOnlyDictionary<string, object?>? Details { get; }

    /// <summary>UTC time the error occurred.</summary>
    public DateTimeOffset OccurredAtUtc { get; }

    public AppException(
        string message,
        AppErrorType type = AppErrorType.Unexpected,
        string? code = null,
        int? httpStatusCode = null,
        IReadOnlyDictionary<string, object?>? details = null,
        Exception? inner = null)
        : base(message, inner)
    {
        Type = type;
        Code = code;
        HttpStatusCode = httpStatusCode ?? type.ToHttpStatusCode();
        Details = details;
        OccurredAtUtc = DateTimeOffset.UtcNow;
    }
}
