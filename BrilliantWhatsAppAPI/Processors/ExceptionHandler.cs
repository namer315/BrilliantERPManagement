using CommonData.Exceptions;
using FastEndpoints;
using FluentValidation.Results;
using System.Text.Json;

namespace BrilliantWhatsAppAPI.Processors
{
    // handle the Exceptions of the Endpoints
    public class ExceptionHandler : IGlobalPostProcessor
    {
        public async Task PostProcessAsync(IPostProcessorContext context , CancellationToken ct)
        {
            if (!context.HasExceptionOccurred)
                return;

            switch (context.ExceptionDispatchInfo.SourceException)
            {
                case NotImplementedException:
                {
                    context.MarkExceptionAsHandled(); //only if handling the exception here.

                    List<ValidationFailure> validationFailures = new List<ValidationFailure>
                    {
                        new ValidationFailure("Message", "This endpoint is not implemented yet!"),
                        new ValidationFailure("Endpoint URL", context.HttpContext.Request.Path)
                    };

                    if (!context.HttpContext.ResponseStarted())
                        await context.HttpContext.Response.SendErrorsAsync(validationFailures , 501);

                    return;
                }
                case UnauthorizedAccessException ex:
                {
                    //Writes a JSON 401 and halts the pipeline.
                    context.MarkExceptionAsHandled();

                    /*List<ValidationFailure> validationFailures = new List<ValidationFailure>
                    {
                        new ValidationFailure("Authorization", ex.Message),
                        new ValidationFailure("Endpoint URL", context.HttpContext.Request.Path)
                    };

                    if (!context.HttpContext.ResponseStarted())
                        await context.HttpContext.Response.SendErrorsAsync(validationFailures, StatusCodes.Status401Unauthorized);*/

                    var response = context.HttpContext.Response;

                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.ContentType = "application/json";

                    //var body = JsonSerializer.Serialize(new { error = ex.Message });
                    var body = JsonSerializer.Serialize(new
                    {
                        error = ex.Message ,
                        //code = ex.Code ,
                        type = AppErrorType.Authentication.ToString() ,
                        //details = ex.Details ,
                        occurredAtUtc = DateTime.UtcNow ,
                        StatusCode = response.StatusCode
                    });

                    await response.WriteAsync(body , ct);
                }
                break;
                case ArgumentNullException ex:

                {
                    Console.WriteLine($"Input error: {ex.Message}");
                }
                break;
                case HttpRequestException ex:
                {
                    Console.WriteLine($"API error: {ex.Message}");
                }
                break;
                case TaskCanceledException:
                {
                    Console.WriteLine("Request timed out. Please check your connection.");
                }
                break;
                case JsonException ex:
                {
                    Console.WriteLine($"Failed to parse response: {ex.Message}");
                }
                break;
                case CommonData.Exceptions.DataAccessException ex:
                {
                    context.MarkExceptionAsHandled();
                    Console.WriteLine($"Data access error: {ex.Message} | Entity={ex.EntityType}, Op={ex.Operation}");

                    if (!context.HttpContext.ResponseStarted())
                    {
                        var response = context.HttpContext.Response;
                        response.StatusCode = StatusCodes.Status500InternalServerError;
                        response.ContentType = "application/json";
                        var body = JsonSerializer.Serialize(new { error = "An internal data error occurred." });
                        await response.WriteAsync(body, ct);
                    }
                }
                break;
                case CommonData.Exceptions.IAppException ex:
                {
                    context.MarkExceptionAsHandled();

                    var response = context.HttpContext.Response;
                    response.StatusCode = ex.HttpStatusCode;
                    response.ContentType = "application/json";

                    var body = JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        code = ex.Code,
                        type = ex.Type.ToString(),
                        details = ex.Details,
                        occurredAtUtc = ex.OccurredAtUtc,
                        StatusCode = ex.HttpStatusCode
                    });

                    await response.WriteAsync(body, ct);
                }
                break;
                case AutoMapper.AutoMapperMappingException ex:
                {
                    context.MarkExceptionAsHandled();

                    var response = context.HttpContext.Response;

                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.ContentType = "application/json";

                    var body = JsonSerializer.Serialize(new { error = ex.Message });

                    await response.WriteAsync(body , ct);
                }
                break;
                case Exception ex:
                {
                    context.MarkExceptionAsHandled();

                    /*List<ValidationFailure> validationFailures = new List<ValidationFailure>
                    {
                        new ValidationFailure("Authorization", ex.Message),
                        new ValidationFailure("Endpoint URL", context.HttpContext.Request.Path)
                    };

                    if (!context.HttpContext.ResponseStarted())
                        await context.HttpContext.Response.SendErrorsAsync(validationFailures, StatusCodes.Status401Unauthorized);*/

                    var response = context.HttpContext.Response;

                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.ContentType = "application/json";

                    var body = JsonSerializer.Serialize(new { error = ex.Message });

                    await response.WriteAsync(body , ct);
                }
                break;
                default:
                    break;
            }

            if (!context.HttpContext.ResponseStarted())
                context.ExceptionDispatchInfo.Throw();
        }
    }
}
