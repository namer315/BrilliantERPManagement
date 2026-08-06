using FastEndpoints;
using FastEndpoints.Swagger;
using BrilliantWhatsAppAPI.Processors;
using BrilliantWhatsAppAPI.Infrastructure;
using CommonData.Session;
using CommonData.DAO;
using CommonData.Services;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── FastEndpoints ──────────────────────────────────────────
        builder.Services.AddFastEndpoints();
        builder.Services.SwaggerDocument();

        // ── HTTP context accessor (needed for tenant resolution) ──
        builder.Services.AddHttpContextAccessor();

        // ── CommonData DAL registrations ───────────────────────────
        builder.Services.AddScoped<ITenantContextAccessor, HttpTenantContextAccessor>();

        // ── In-memory tenant cache (token -> tenant, DB fallback) ──
        builder.Services.AddSingleton<TenantCacheService>();

        //connect to the database before starting the application
        await DataBaseConnect();

        var app = builder.Build();

        // ── Initialize NHibernate eagerlly (before any DB access) ──
        app.UseSwaggerGen();

        // Wrap the endpoint pipeline so the ambient TenantContext is always
        // cleared when the request ends (must run BEFORE UseFastEndpoints).
        app.UseMiddleware<TenantScopeCleaner>();

        app.UseFastEndpoints(c =>
        {
            c.Endpoints.Configurator = ep =>
            {
                ep.PreProcessor<TokenPreProcessor>(Order.Before);
                //ep.PreProcessor<TenantPreProcessor>(Order.Before);
                //ep.PostProcessor<TenantPreProcessor>(Order.After);
                ep.PostProcessor<ExceptionHandler>(Order.After);
            };
            c.Endpoints.RoutePrefix = "WhatsAppAPI";
        });

        // Seed the in-memory tenant cache from the DB at startup.
        app.Services.GetRequiredService<TenantCacheService>().Warmup();

        app.MapGet("/" , async context =>
        {
            context.Response.Redirect("/swagger");
            await Task.CompletedTask;
        });

        app.Run();
    }

    private static async Task DataBaseConnect()
    {

        Connection connection = new Connection()
        {
            Server = "DESKTOP-PJCEMGK" ,
            DataBaseName = "BrilliantWhatsApp" ,
            User = "sa" ,
            Password = "123456" ,
            DataBaseKind = Connection.DataBaseKinds.SQLServer ,
        };
        connection.SessionConnect_Login();
        await connection.SessionConnect();
        await Connection.DataBaseUpdate(connection);
    }
}