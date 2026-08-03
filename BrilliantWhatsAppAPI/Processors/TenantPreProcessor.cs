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
    private readonly NHibernateUnitOfWork _uow;

    public TenantPreProcessor(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        _uow.Begin();
        await Task.CompletedTask;
    }

    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        if (context.HasExceptionOccurred)
        {
            // Dispose triggers rollback since we never committed
            _uow.Dispose();
        }
        else
        {
            _uow.Commit();
            _uow.Dispose();
        }
        await Task.CompletedTask;
    }
}
