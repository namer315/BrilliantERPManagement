using System.Text;
using System.Text.Json;

namespace CommonData.Session;

/// <summary>
/// Describes a single database connection used to build the NHibernate session factory.
/// Supports SQL Server, MySQL and Oracle. The database kind is auto-detected from the
/// connection string so you only change config to point at a different database.
/// </summary>
public class Connection
{
    public string Server { get; set; } = ".";
    public string DataBaseName { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DataBaseKinds DataBaseKind { get; set; } = DataBaseKinds.SQLServer;

    public static Connection CurrentConnection { get; set; }

    /// <summary>
    /// to enable NHibernate Envers
    /// </summary>
    public bool IsAudited { get; set; }


    public string UseSlidingExpiration { get; set; } = "true";

    /// <summary>
    /// it uses as Interval in Minutes for the reminder
    /// </summary>
    public string DefaultExpiration { get; set; } = "300";

    public string command_timeout { get; set; } = "90";

    public enum DataBaseKinds
    {
        SQLServer = 0,
        MySql = 1,
        PostgreSQL = 2,
        SQLite = 3,
        Oracel = 4,
        Firebird = 5,
        SybaseASE = 6,
    }

    #region methods
    public void SessionConnect_Login(Connection connection)
    {
        new SessionFactoryGenerator().CreateSessionFactory_Login(connection);
    }
    public void SessionConnect_Login()
    {
        SessionConnect_Login(this);
    }

    public async Task SessionConnect(Connection connection)
    {
        await new SessionFactoryGenerator().CreateSessionFactory(connection);
        CurrentConnection = connection;
        //RepositoryBase.GetSession();
    }
    public async Task SessionConnect()
    {
        await SessionConnect(this);
    }

    public static async Task<string> DataBaseUpdate(Connection connection)
    {
        StringBuilder msg = new StringBuilder();
        try
        {
            await SessionFactoryGenerator.DataBaseUpdate(connection);

            msg.Append(" DataBase : \n --------------  " + connection.DataBaseName + "  -------------- \n has been updated successfully");
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
                msg.Append(ex.InnerException.Message);
            else
                msg.Append(ex.ToString());
        }

        return msg.ToString();
    }

    /// <summary>
    /// Reads the Connection model from config.json.
    /// If the file doesn't exist, creates a new default Connection,
    /// saves it to config.json, and returns it.
    /// </summary>
    public static Connection LoadOrCreateConfig(string? filePath = null)
    {
        filePath ??= Path.Combine(AppContext.BaseDirectory, "config.json");

        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Connection>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new Connection();
        }

        // File doesn't exist — create a default model, save it, and return it
        Connection defaultConnection = new Connection()
        {
            Server = Environment.MachineName ,
            DataBaseName = "BrilliantWhatsApp" ,
        };
        File.WriteAllText(filePath,
            JsonSerializer.Serialize(defaultConnection,
                new JsonSerializerOptions { WriteIndented = true }));

        throw new Exception(
                "config.json has been created with default values. " +
                "Please configure your database connection (Server, DataBaseName, User, Password, DataBaseKind) " +
                "in config.json, then restart the application.");
        //return defaultConnection;
    }

    /// <summary>
    /// Loads (or creates) the connection config from config.json, then
    /// connects to the database and applies any pending schema updates.
    /// </summary>
    public static async Task DataBaseConnect()
    {
        Connection connection = LoadOrCreateConfig();
        connection.SessionConnect_Login();
        await connection.SessionConnect();
        await DataBaseUpdate(connection);
    }

    #endregion
}
