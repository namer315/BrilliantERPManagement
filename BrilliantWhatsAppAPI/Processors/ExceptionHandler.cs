//using AutoMapper;
//using Business;
//using Data;
//using Data.VO.Commons;
//using DTO.Common.Permission;
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

                    var body = JsonSerializer.Serialize(new { error = ex.Message });

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
                /*catch (ArgumentNullException ex)
        {
                    await SendErrorsAsync(400 , ct); // Missing WABA ID or token
                }
        catch (HttpRequestException ex) when(ex.Message.Contains("401"))
        {
                    await SendErrorsAsync(401 , ct); // Invalid/expired token
                }
        catch (HttpRequestException ex) when(ex.Message.Contains("404"))
        {
                    await SendErrorsAsync(404 , ct); // Wrong WABA ID
                }
        catch (TaskCanceledException)
        {
                    await SendErrorsAsync(504 , ct); // Timeout
                }*/

                default:
                    break;
            }

            if (!context.HttpContext.ResponseStarted())
                context.ExceptionDispatchInfo.Throw();
        }
    }
}
