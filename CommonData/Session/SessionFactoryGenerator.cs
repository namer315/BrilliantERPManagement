using CommonData.DAO;
using CommonData.DAO.FNHConfig;
using CommonData.Services;
using CommonData.VO;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Data.SqlClient;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Driver;
using NHibernate.Envers.Configuration.Attributes;
using NHibernate.Loader;
using NHibernate.Tool.hbm2ddl;
using NLog;

namespace CommonData.Session;

public class SessionFactoryGenerator
{
    public static ISessionFactory SessionFactory = null!;
    public static bool UpdateDataBase;
    public static bool _isAudited { get; set; }
    private Connection _connection = null!;

    private static readonly Dictionary<Connection.DataBaseKinds , Func<Connection , IPersistenceConfigurer>> DatabaseConfigurations = new Dictionary<Connection.DataBaseKinds , Func<Connection , IPersistenceConfigurer>>
{
{
Connection.DataBaseKinds.SQLServer, connection => getSQLVersion(connection.Server, connection.DataBaseName, connection.User, connection.Password)
.ConnectionString(SqlServerConnectionString(connection))
.Driver<MicrosoftDataSqlClientDriver>()
.MaxFetchDepth(2)
.UseReflectionOptimizer()
#if DEBUG
//.ShowSql()
//.FormatSql()
#endif
.IsolationLevel(System.Data.IsolationLevel.ReadCommitted)
},
{
Connection.DataBaseKinds.MySql, connection => MySQLConfiguration.Standard
.ConnectionString(x => x.Server(connection.Server)
.Database(connection.DataBaseName)
.Username(connection.User)
.Password(connection.Password))
},
{
Connection.DataBaseKinds.Oracel, connection => OracleManagedDataClientConfiguration.Oracle10
.ConnectionString(x => x.Server(connection.Server)
.Instance(connection.DataBaseName)
.Username(connection.User)
.Password(connection.Password))
},
{
Connection.DataBaseKinds.PostgreSQL, connection => PostgreSQLConfiguration.PostgreSQL82
.ConnectionString(x => x.Host(connection.Server)
.Database(connection.DataBaseName)
.Username(connection.User)
.Password(connection.Password))
},
{
Connection.DataBaseKinds.SQLite, connection => SQLiteConfiguration.Standard
.UsingFile(connection.DataBaseName)
}
};

    // Cache the last used configuration hash to avoid unnecessary rebuilds
    private static string _lastConfigHash = null!;
    private static readonly object _sessionFactoryLock = new object();

    // Cache audited types to avoid repeated reflection scans
    private static readonly Lazy<Type[]> _cachedAuditedTypes = new Lazy<Type[]>(() =>
    typeof(TenantVO).Assembly
    .GetTypes()
    .Where(t => t.IsDefined(typeof(AuditedAttribute) , inherit: true))
    .ToArray() ,
    LazyThreadSafetyMode.ExecutionAndPublication);

    // Cache for SQL version per server to avoid repeated connections
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string , MsSqlConfiguration> _sqlVersionCache
    = new System.Collections.Concurrent.ConcurrentDictionary<string , MsSqlConfiguration>();
    // Semaphore to ensure only one session-factory build runs at a time (async-friendly)
    private static readonly SemaphoreSlim _factoryBuildSemaphore = new SemaphoreSlim(1 , 1);

    public async Task<ISession> CreateSessionFactory(Connection connection)
    {
        _isAudited = connection.IsAudited;
        _connection = connection;

        if (!DatabaseConfigurations.TryGetValue(connection.DataBaseKind , out var configurer))
        {
            throw new ArgumentException($"Unsupported database kind: {connection.DataBaseKind}");
        }

        // Compute a hash of the connection/configuration to avoid unnecessary rebuilds
        string configHash = $"{connection.DataBaseKind}|{connection.Server}|{connection.DataBaseName}|{connection.User}|{connection.Password}|{connection.IsAudited}|{connection.UseSlidingExpiration}|{connection.DefaultExpiration}|{connection.command_timeout}";

        // Fast path: check if we can reuse existing factory without locking
        var existingFactory = SessionFactory;
        if (existingFactory != null && _lastConfigHash == configHash)
        {
            var sfImpl = existingFactory as NHibernate.Impl.SessionFactoryImpl;
            if (sfImpl != null && !sfImpl.IsClosed)
            {
                return existingFactory.OpenSession();
            }
        }

        // Pre-warm SQL version cache outside the lock for SQL Server using async Open
        if (connection.DataBaseKind == Connection.DataBaseKinds.SQLServer)
        {
            try
            {
                // Use true async open to avoid threadpool blocking
                await GetOrCacheSqlVersionAsync_Async(connection.Server , connection.DataBaseName , connection.User , connection.Password).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log and continue with default - do not throw, keep behavior compatible
                CurrentLogger.Instance.Error($"Pre-warming SQL version cache failed: {ex.Message}" , ex);
            }
        }

        // Pre-warm audited types cache if auditing is enabled
        if (_isAudited)
        {
            _ = _cachedAuditedTypes.Value; // Lazy ensures one-time reflection
        }

        // Fast-path again to avoid waiting for semaphore if another thread already built factory
        existingFactory = SessionFactory;
        if (existingFactory != null && _lastConfigHash == configHash)
        {
            var sfImpl = existingFactory as NHibernate.Impl.SessionFactoryImpl;
            if (sfImpl != null && !sfImpl.IsClosed)
            {
                return existingFactory.OpenSession();
            }
        }

        ISessionFactory? builtFactory = null;

        // Use async semaphore to ensure single factory build at a time
        await _factoryBuildSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            // Double-check after acquiring semaphore
            existingFactory = SessionFactory;
            if (existingFactory != null && _lastConfigHash == configHash)
            {
                var sfImpl = existingFactory as NHibernate.Impl.SessionFactoryImpl;
                if (sfImpl != null && !sfImpl.IsClosed)
                {
                    return existingFactory.OpenSession();
                }
            }

            // Build configuration outside any long-held locks. FluentNHibernate's BuildSessionFactory is CPU/IO-bound.
            var configuration = Fluently.Configure()
            .Database(configurer(connection))
            .Mappings(m =>
            {
                m.UsePrecompiledModel();
                m.FluentMappings.Add<GlobalConditionFilter>();
            })
            .ExposeConfiguration(cfg =>
            {
                // Reduce overhead by tuning NHibernate settings for faster startup
                cfg.SetProperty(NHibernate.Cfg.Environment.PropertyUseReflectionOptimizer , "true");
                cfg.SetProperty(NHibernate.Cfg.Environment.DefaultBatchFetchSize , "50");
                cfg.SetProperty(NHibernate.Cfg.Environment.BatchSize , "50"); // adonet.batch_size
                cfg.SetProperty("prepare_sql" , "true");
                cfg.SetProperty("use_sql_comments" , "false");
                cfg.SetProperty("generate_statistics" , "false");
                cfg.SetProperty(NHibernate.Cfg.Environment.ReleaseConnections , "after_transaction");

                if (connection.DataBaseKind == Connection.DataBaseKinds.SQLServer)
                {
                    cfg.SetProperty(NHibernate.Cfg.Environment.QuerySubstitutions , "true 1, false 0, yes 'Y', no 'N'");
                }

                BuildSchema(cfg);
            });

            if (connection.DataBaseKind != Connection.DataBaseKinds.PostgreSQL &&
            connection.DataBaseKind != Connection.DataBaseKinds.SQLite)
            {
                configuration = configuration.Cache(c => c
                .UseSecondLevelCache()
                .UseQueryCache()
                                        .ProviderClass<NHibernate.Cache.HashtableCacheProvider>());
            }

            // Dispose previous SessionFactory if exists
            if (SessionFactory != null)
            {
                try { SessionFactory.Dispose(); } catch { }
            }

            // Building the session factory is the expensive operation. Keep it inside semaphore but avoid other locks.
            builtFactory = configuration.BuildSessionFactory();

            // Exchange the global reference
            SessionFactory = builtFactory;
            _lastConfigHash = configHash;
        }
        finally
        {
            _factoryBuildSemaphore.Release();
        }

        return builtFactory.OpenSession();
    }

    public void CreateSessionFactory_Login(Connection connection)
    {
        try
        {
            Fluently.Configure()
            .Database(getSQLVersion(connection.Server , connection.DataBaseName , connection.User , connection.Password)
            .ConnectionString(SqlServerConnectionString(connection))
            .Driver<MicrosoftDataSqlClientDriver>())
            .Mappings(LoginMapping)
            .BuildSessionFactory();
        }
        catch
        {
        }
    }

    private void LoginMapping(MappingConfiguration mappingConfiguration)
    {
        mappingConfiguration.FluentMappings.Add<TenantMap>();
    }

    public static MsSqlConfiguration sqlConfiguration = null!;

    /// <summary>
    /// Gets or caches the SQL Server version configuration for a given server.
    /// Uses a per-server cache to support multiple database connections.
    /// </summary>
    private static MsSqlConfiguration GetOrCacheSqlVersionAsync(string server , string dataBase , string sqlUserName , string sqlPass)
    {
        // Create a cache key based on the server (version is server-specific, not database-specific)
        string cacheKey = server.ToLowerInvariant();

        return _sqlVersionCache.GetOrAdd(cacheKey , _ =>
        {
            try
            {
                // Use connection pooling and proper disposal
                using var sqlConnection = new SqlConnection(
                $"Server={server};Database={dataBase};user={sqlUserName};password={sqlPass};" +
                "MultipleActiveResultSets=true;Connection Timeout=15;Pooling=true;TrustServerCertificate=True;");
                sqlConnection.Open();
                string versionNo = sqlConnection.ServerVersion.Substring(0 , 2);

                return versionNo switch
                {
                    "9" => MsSqlConfiguration.MsSql2005,
                    "10" => MsSqlConfiguration.MsSql2008,
                    "11" or "12" or "13" or "14" or "15" or "16" => MsSqlConfiguration.MsSql2012,
                    _ => MsSqlConfiguration.MsSql2012 // Default to 2012 for modern SQL Server versions
                };
            }
            catch (Exception ex)
            {
                CurrentLogger.Instance.Error($"Failed to detect SQL Server version for {server}: {ex.Message}" , ex);
                return MsSqlConfiguration.MsSql2012; // Default to 2012 for better compatibility
            }
        });
    }

    // Async variant that uses OpenAsync to avoid synchronous blocking on threadpool
    private static async Task<MsSqlConfiguration> GetOrCacheSqlVersionAsync_Async(string server , string dataBase , string sqlUserName , string sqlPass)
    {
        string cacheKey = server.ToLowerInvariant();

        if (_sqlVersionCache.TryGetValue(cacheKey , out var cached))
            return cached;

        try
        {
            using var sqlConnection = new SqlConnection(
            $"Server={server};Database={dataBase};user={sqlUserName};password={sqlPass};" +
            "MultipleActiveResultSets=true;Connection Timeout=15;Pooling=true;TrustServerCertificate=True;");
            await sqlConnection.OpenAsync().ConfigureAwait(false);
            string versionNo = sqlConnection.ServerVersion.Substring(0 , 2);

            var config = versionNo switch
            {
                "9" => MsSqlConfiguration.MsSql2005,
                "10" => MsSqlConfiguration.MsSql2008,
                "11" or "12" or "13" or "14" or "15" or "16" => MsSqlConfiguration.MsSql2012,
                _ => MsSqlConfiguration.MsSql2012
            };

            _sqlVersionCache.TryAdd(cacheKey , config);
            return config;
        }
        catch (Exception ex)
        {
            CurrentLogger.Instance.Error($"Failed to detect SQL Server version for {server}: {ex.Message}" , ex);
            return MsSqlConfiguration.MsSql2012;
        }
    }

    private static MsSqlConfiguration getSQLVersion(string server , string dataBase , string sqlUserName , string sqlPass)
    {
        // Maintain backward compatibility while using the new cached method
        if (sqlConfiguration != null)
            return sqlConfiguration;

        sqlConfiguration = GetOrCacheSqlVersionAsync(server , dataBase , sqlUserName , sqlPass);
        return sqlConfiguration;
    }

    /// <summary>
    /// Builds a SQL Server connection string with TrustServerCertificate enabled so
    /// that local/dev connections to self-signed certificates don't fail validation.
    /// </summary>
    private static string SqlServerConnectionString(Connection connection)
    {
        return new SqlConnectionStringBuilder
        {
            DataSource = connection.Server,
            InitialCatalog = connection.DataBaseName,
            UserID = connection.User,
            Password = connection.Password,
            MultipleActiveResultSets = true,
            TrustServerCertificate = true,
        }.ConnectionString;
    }

    private void BuildSchema(Configuration config)
    {
        if (_isAudited)
        {
            var fluentConfiguration = new NHibernate.Envers.Configuration.Fluent.FluentConfiguration();

            // Use cached audited types to avoid repeated reflection scans
            foreach (Type currentType in _cachedAuditedTypes.Value)
            {
                fluentConfiguration.Audit(currentType);
            }

            config.SetProperty("nhibernate.envers.track_entities_changed_in_revision" , "true");
            config.SetProperty("nhibernate.envers.revision_on_collection_change" , "false");
            config.SetProperty("nhibernate.envers.modified_flag_suffix" , "");
            config.IntegrateWithEnvers(fluentConfiguration);

            config.Proxy(p => p.ProxyFactoryFactory<NHibernate.Bytecode.StaticProxyFactoryFactory>());
        }
        config.SetProperty(NHibernate.Cfg.Environment.BatchFetchStyle , BatchFetchStyle.Dynamic.ToString());
        config.SetProperty("cache.use_sliding_expiration" , _connection.UseSlidingExpiration);
        config.SetProperty("cache.default_expiration" , _connection.DefaultExpiration);
        config.SetProperty("command_timeout" , _connection.command_timeout);
        config.SetProperty("connection.connection_timeout" , _connection.command_timeout);
        RepositoryBase._command_timeout = int.TryParse(_connection.command_timeout , out var timeout) ? timeout : 90;
        // OPTION 1: Ensure your session factory creates connections that stay open
        // In your NHibernate configuration:
        config.SetProperty(NHibernate.Cfg.Environment.ConnectionProvider ,
        typeof(NHibernate.Connection.DriverConnectionProvider).FullName);
        //config.SetProperty(NHibernate.Cfg.Environment.ReleaseConnections , "on_close");

        //config.RegisterDisableAutoDirtyCheckListeners();

        if (UpdateDataBase)
        {
            if (SessionFactory != null)
            {
                // Check if SessionFactory is disposed
                var sfImpl = SessionFactory as NHibernate.Impl.SessionFactoryImpl;
                if (sfImpl == null || sfImpl.IsClosed)
                {
                    // Rebuild SessionFactory if disposed
                    SessionFactory = Fluently.Configure()
                        .Database(DatabaseConfigurations[_connection.DataBaseKind](_connection))
                        .Mappings(m =>
                        {
                            m.UsePrecompiledModel();
                            m.FluentMappings.Add<GlobalConditionFilter>();
                        })
                        .BuildSessionFactory();
                }
                SessionFactory.OpenSession();
            }
            SchemaUpdate su = new SchemaUpdate(config);
            su.ExecuteAsync(true , true);
        }
    }

    public static async Task DataBaseUpdate(Connection connection)
    {
        if (SessionFactory != null)
        {
            SessionFactory.Dispose();
        }

        UpdateDataBase = true;
        var sessionFactoryGenerator = new SessionFactoryGenerator();
        await Task.Run(() => sessionFactoryGenerator.CreateSessionFactory(connection));
        UpdateDataBase = false;

        // Handle session operations asynchronously
        if (RepositoryBase.Session != null)
        {
            RepositoryBase.Session.Clear();
            await RepositoryBase.Session.FlushAsync();
        }

        if (Connection.CurrentConnection != null)
        {
            await Task.Run(() => new SessionFactoryGenerator().CreateSessionFactory(Connection.CurrentConnection));
        }
        else
        {
            await Task.Run(() => new SessionFactoryGenerator().CreateSessionFactory(connection));
        }

        if (RepositoryBase.Session != null)
        {
            RepositoryBase.Session.Close();
        }
    }

    public static void garbageCollector()
    {
        // Forced GC removed — let the runtime manage collection.
        // Explicit GC.Collect() causes full blocking collections that
        // pause all threads and hurt throughput without measurable benefit.
    }
}
