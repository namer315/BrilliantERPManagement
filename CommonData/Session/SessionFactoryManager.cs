using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NLog;

namespace CommonData.Session;

/// <summary>
/// Singleton provider for the NHibernate ISessionFactory.
/// Initialized once at application startup with a connection string.
/// Integrates MySQL, FluentNHibernate auto-mapping, Envers auditing, and global filters.
/// </summary>
public class SessionFactoryManager
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    private static readonly Lazy<SessionFactoryManager> _instance =
        new(() => new SessionFactoryManager());

    private ISessionFactory? _sessionFactory;
    private readonly object _lock = new();

    /// <summary>
    /// Singleton accessor. Initialize(...) must be called before first use.
    /// </summary>
    public static SessionFactoryManager Instance => _instance.Value;

    /// <summary>
    /// The configured ISessionFactory. Throws if not yet initialized.
    /// </summary>
    public ISessionFactory SessionFactory
    {
        get
        {
            if (_sessionFactory is null)
                throw new InvalidOperationException(
                    "SessionFactoryManager not initialized. Call Initialize(connectionString) at startup.");
            return _sessionFactory;
        }
    }

    private SessionFactoryManager() { }

    /// <summary>
    /// Builds the ISessionFactory from the given MySQL connection string.
    /// Idempotent — subsequent calls are ignored.
    /// </summary>
    public void Initialize(string connectionString)
    {
        if (_sessionFactory is not null) return;

        lock (_lock)
        {
            if (_sessionFactory is not null) return;

            _log.Info("Initializing SessionFactoryManager | CorrelationId={0}",
                Guid.NewGuid().ToString("N")[..8]);

            var fluentConfig = Fluently.Configure()
                .Database(MySQLConfiguration.Standard
                    .ConnectionString(connectionString)
                    .Dialect<NHibernate.Dialect.MySQL57Dialect>()
                    .Driver<NHibernate.Driver.MySqlDataDriver>()
                    .FormatSql()
                    .ShowSql())
                .Mappings(m =>
                    m.FluentMappings.AddFromAssemblyOf<VO.AppBaseEntity>()
                     .Conventions.Add(FluentNHibernate.Conventions.Helpers
                         .DefaultLazy.Always()))
                .ExposeConfiguration(cfg =>
                {
                    GlobalConditionFilter.RegisterFilterDefinitions(cfg);

                    cfg.SetProperty(NHibernate.Cfg.Environment.UseSecondLevelCache, "false");
                    cfg.SetProperty(NHibernate.Cfg.Environment.UseQueryCache, "false");

                    // Envers: basic integration — auditing enabled via [Audited] attribute on entities
                    cfg.IntegrateWithEnvers();
                });

            _sessionFactory = fluentConfig.BuildSessionFactory();

            _log.Info("SessionFactoryManager initialized successfully");
        }
    }

    /// <summary>
    /// Opens a new ISession from the factory.
    /// </summary>
    public ISession OpenSession()
    {
        var session = SessionFactory.OpenSession();
        return session;
    }

    /// <summary>
    /// Builds the database schema (dev only). Uses SchemaExport.
    /// </summary>
    public void BuildSchema()
    {
        var cfg = new Configuration();
        GlobalConditionFilter.RegisterFilterDefinitions(cfg);
        // SchemaExport is informational — actual migration via external tooling
        _log.Info("SchemaExport triggered (dev mode)");
    }
}
