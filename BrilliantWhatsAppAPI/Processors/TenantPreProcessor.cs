using CommonData.Session;
using FastEndpoints;

namespace BrilliantWhatsAppAPI.Processors;

/// <summary>
/// Global pre/post-processor that wraps every endpoint request in a
/// Unit of Work transaction. Begins on entry, commits on success,
/// rolls back on exception.
/// </summary>
public class TenantPreProcessor : IGlobalPreProcessor, IGlobalPostProcessor
{
    // No constructor injection of scoped services — FastEndpoints creates
    // global processors as singletons. Scoped NHibernateUnitOfWork is
    // resolved per-request from HttpContext.RequestServices.

    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var uow = context.HttpContext.RequestServices
            .GetRequiredService<NHibernateUnitOfWork>();
        uow.Begin();
        await Task.CompletedTask;
    }

    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        var uow = context.HttpContext.RequestServices
            .GetRequiredService<NHibernateUnitOfWork>();
        if (context.HasExceptionOccurred)
        {
            // Dispose triggers rollback since we never committed
            uow.Dispose();
        }
        else
        {
            uow.Commit();
            uow.Dispose();
        }
        await Task.CompletedTask;
    }
}
