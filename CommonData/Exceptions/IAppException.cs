namespace CommonData.Exceptions;

/// <summary>
/// Contract that every project-specific exception should implement, so the API
/// layer can translate any thrown error into a consistent HTTP response no
/// matter which API project raised it.
/// </summary>
public interface IAppException
{
    /// <summary>Human-readable error message.</summary>
    string Message { get; }

    /// <summary>Machine-readable error code (e.g. "TENANT_NOT_FOUND").</summary>
    string? Code { get; }

    /// <summary>Category of the error.</summary>
    AppErrorType Type { get; }

    /// <summary>HTTP status code returned to the caller.</summary>
    int HttpStatusCode { get; }

    /// <summary>Optional structured details for the client / logs.</summary>
    IReadOnlyDictionary<string, object?>? Details { get; }

    /// <summary>UTC time the error occurred.</summary>
    DateTimeOffset OccurredAtUtc { get; }
}
