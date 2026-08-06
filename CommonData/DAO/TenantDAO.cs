using CommonData.Exceptions;
using CommonData.Session;
using CommonData.VO;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using Polly;

namespace CommonData.DAO;

/// <summary>
/// Data Access Object for TenantVO.
/// All methods use the session from the scoped NHibernateUnitOfWork.
/// Retry policy (3 attempts, exponential backoff) applied for transient DB errors.
/// Never commits — transaction boundaries managed by the Unit of Work.
/// </summary>
public class TenantDAO
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    private readonly NHibernateUnitOfWork _uow;

    private static readonly ResiliencePipeline _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(200),
            BackoffType = DelayBackoffType.Exponential,
            OnRetry = args =>
            {
                LogManager.GetCurrentClassLogger()
                    .Warn("Retry {0} for DAO operation after {1}ms | Exception: {2}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                return ValueTask.CompletedTask;
            }
        })
        .Build();

    private ISession Session => _uow.Session;

    public TenantDAO(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// Retrieves a tenant by its primary key.
    /// </summary>
    public TenantVO? GetById(Guid id)
    {
        try
        {
            _log.Debug("TenantDAO.GetById({0})", id);
            return _retryPipeline.Execute(() => Session.Get<TenantVO>(id));
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            _log.Error(ex, "TenantDAO.GetById({0}) failed", id);
            throw new DataAccessException(
                $"Failed to get TenantVO by Id={id}", ex,
                nameof(TenantVO), nameof(GetById));
        }
    }

    /// <summary>
    /// Retrieves a tenant by its authentication token.
    /// </summary>
    public TenantVO? GetByToken(string token)
    {
        try
        {
            _log.Debug("TenantDAO.GetByToken({0})", token[..Math.Min(token.Length, 8)]);
            return _retryPipeline.Execute(() =>
                Session.QueryOver<TenantVO>()
                    .Where(t => t.Token == token)
                    .SingleOrDefault());
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            _log.Error(ex, "TenantDAO.GetByToken failed");
            throw new DataAccessException(
                "Failed to get TenantVO by token", ex,
                nameof(TenantVO), nameof(GetByToken));
        }
    }

    /// <summary>
    /// Retrieves a tenant by its name.
    /// </summary>
    public TenantVO? GetByName(string name)
    {
        try
        {
            _log.Debug("TenantDAO.GetByName({0})", name);
            return _retryPipeline.Execute(() =>
                Session.QueryOver<TenantVO>()
                    .Where(t => t.Name == name)
                    .SingleOrDefault());
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            _log.Error(ex, "TenantDAO.GetByName failed");
            throw new DataAccessException(
                $"Failed to get TenantVO by name={name}", ex,
                nameof(TenantVO), nameof(GetByName));
        }
    }

    /// <summary>
    /// Searches tenants using the fluent TenantSearch filter builder.
    /// </summary>
    public IList<TenantVO> Search(Search.TenantSearch search)
    {
        try
        {
            _log.Debug("TenantDAO.Search | Filters={0}", search.HasFilters);
            return _retryPipeline.Execute(() =>
            {
                var query = Session.QueryOver<TenantVO>();
                return search.Apply(query).List();
            });
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            _log.Error(ex, "TenantDAO.Search failed");
            throw new DataAccessException(
                "Failed to search TenantVO", ex,
                nameof(TenantVO), nameof(Search));
        }
    }

    /// <summary>
    /// Returns all active tenants.
    /// </summary>
    public IList<TenantVO> GetAllActive()
    {
        try
        {
            _log.Debug("TenantDAO.GetAllActive");
            return _retryPipeline.Execute(() =>
                Session.QueryOver<TenantVO>()
                    .Where(t => t.Active)
                    .List());
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            _log.Error(ex, "TenantDAO.GetAllActive failed");
            throw new DataAccessException(
                "Failed to get all active tenants", ex,
                nameof(TenantVO), nameof(GetAllActive));
        }
    }

    /// <summary>
    /// Persists a new tenant or updates an existing one.
    /// </summary>
    public void Save(TenantVO tenant)
    {
        try
        {
            _log.Debug("TenantDAO.Save | Id={0}, Name={1}", tenant.Id, tenant.Name);
            _retryPipeline.Execute(() =>
            {
                Session.SaveOrUpdate(tenant);
            });
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            _log.Error(ex, "TenantDAO.Save failed | Name={0}", tenant.Name);
            throw new DataAccessException(
                $"Failed to save TenantVO Name={tenant.Name}", ex,
                nameof(TenantVO), nameof(Save));
        }
    }

    /// <summary>
    /// Soft-deletes a tenant by setting IsDeleted=true.
    /// </summary>
    public void Delete(Guid id)
    {
        try
        {
            _log.Debug("TenantDAO.Delete({0})", id);
            _retryPipeline.Execute(() =>
            {
                var tenant = Session.Get<TenantVO>(id);
                if (tenant is not null)
                {
                    tenant.IsDeleted = true;
                    tenant.UpdatedAt = DateTime.UtcNow;
                    Session.Update(tenant);
                }
            });
        }
        catch (Exception ex) when (ex is not DataAccessException)
        {
            _log.Error(ex, "TenantDAO.Delete({0}) failed", id);
            throw new DataAccessException(
                $"Failed to delete TenantVO Id={id}", ex,
                nameof(TenantVO), nameof(Delete));
        }
    }
}
