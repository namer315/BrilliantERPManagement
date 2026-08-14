namespace CommonData.Exceptions;

/// <summary>
/// High-level category of an application error. The integer value of each member
/// is the default HTTP status code returned for that category.
/// </summary>
public enum AppErrorType
{
    /// <summary>Request failed input validation. (400)</summary>
    Validation = 400,

    /// <summary>Missing or invalid credentials. (401)</summary>
    Authentication = 401,

    /// <summary>Authenticated but not permitted. (403)</summary>
    Authorization = 403,

    /// <summary>Requested resource was not found. (404)</summary>
    NotFound = 404,

    /// <summary>State conflict (duplicate, stale version, ...). (409)</summary>
    Conflict = 409,

    /// <summary>A domain/business rule was violated. (422)</summary>
    BusinessRule = 422,

    /// <summary>Data-access / persistence failure. (500)</summary>
    DataAccess = 500,

    /// <summary>An external / downstream service failed. (502)</summary>
    ExternalService = 502,

    /// <summary>Unhandled, unexpected failure. (500)</summary>
    Unexpected = 500,
}

/// <summary>
/// Helpers for <see cref="AppErrorType"/>.
/// </summary>
public static class AppErrorTypeExtensions
{
    /// <summary>Returns the default HTTP status code for the error type.</summary>
    public static int ToHttpStatusCode(this AppErrorType type) => (int)type;
}
