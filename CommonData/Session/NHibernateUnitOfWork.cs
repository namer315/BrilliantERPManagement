using NHibernate;
using NLog;

namespace CommonData.Session;

/// <summary>
/// Scoped Unit of Work wrapping an NHibernate ISession and ITransaction.
///
/// Usage (per request):
///   1. uow.Begin()      — opens session + starts transaction + enables filters
///   2. DAO operations   — use uow.Session for queries
///   3. uow.Commit()     — flushes and commits
///   On dispose: auto-rollback if not committed.
/// </summary>
public class NHibernateUnitOfWork : IDisposable
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    private readonly SessionFactoryManager _sfm;
    private readonly ITenantContextAccessor _tenantAccessor;

    private ISession? _session;
    private ITransaction? _transaction;
    private bool _committed;
    private bool _disposed;

    /// <summary>
    /// The active NHibernate session. Valid after Begin() is called.
    /// </summary>
    public ISession Session =>
        _session ?? throw new InvalidOperationException(
            "Session not open. Call Begin() before accessing Session.");

    public NHibernateUnitOfWork(
        SessionFactoryManager sfm,
        ITenantContextAccessor tenantAccessor)
    {
        _sfm = sfm;
        _tenantAccessor = tenantAccessor;
    }

    /// <summary>
    /// Opens a session, begins a transaction, and enables global filters.
    /// </summary>
    public void Begin()
    {
        if (_session is not null) return;

        _session = _sfm.OpenSession();
        _transaction = _session.BeginTransaction();

        GlobalConditionFilter.EnableSoftDelete(_session);

        var tenant = _tenantAccessor.CurrentTenant;
        if (tenant is not null)
        {
            GlobalConditionFilter.EnableTenantFilter(_session, tenant.Id);
        }

        _log.Debug("UoW Begin | SessionId={0}", _session.GetHashCode());
    }

    /// <summary>
    /// Flushes the session and commits the transaction.
    /// </summary>
    public void Commit()
    {
        if (_transaction?.IsActive == true)
        {
            _session?.Flush();
            _transaction.Commit();
            _committed = true;
            _log.Debug("UoW Committed | SessionId={0}", _session?.GetHashCode());
        }
    }

    /// <summary>
    /// Rolls back if not committed, then closes the session.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (!_committed && _transaction?.IsActive == true)
        {
            _log.Warn("UoW rolled back (not committed) | SessionId={0}",
                _session?.GetHashCode());
            _transaction.Rollback();
        }

        _transaction?.Dispose();
        _session?.Dispose();
    }
}
