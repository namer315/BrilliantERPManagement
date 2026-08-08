namespace BrilliantWhatsAppAPI.Infrastructure;

public sealed class RequestBodyBufferingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestBodyBufferingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        await _next(context);
    }
}
