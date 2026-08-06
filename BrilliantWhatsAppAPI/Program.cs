using FastEndpoints;
using FastEndpoints.Swagger;
using BrilliantWhatsAppAPI.Processors;
using BrilliantWhatsAppAPI.Infrastructure;
using CommonData.Session;
using CommonData.DAO;
using CommonData.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── FastEndpoints ──────────────────────────────────────────
        builder.Services.AddFastEndpoints();
        builder.Services.SwaggerDocument();

        // ── HTTP context accessor (needed for tenant resolution) ──
        builder.Services.AddHttpContextAccessor();

        // ── CommonData DAL registrations ───────────────────────────
        builder.Services.AddScoped<ITenantContextAccessor, HttpTenantContextAccessor>();
        builder.Services.AddSingleton<SessionFactoryManager>(sp =>
        {
            var sfm = SessionFactoryManager.Instance;
            var connStr = builder.Configuration.GetConnectionString("ERP")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:ERP is not configured.");
            sfm.Initialize(connStr);
            return sfm;
        });
        builder.Services.AddScoped<NHibernateUnitOfWork>();
        builder.Services.AddScoped<TenantDAO>();

        // ── In-memory tenant cache (token -> tenant, DB fallback) ──
        builder.Services.AddSingleton<TenantCacheService>();

        var app = builder.Build();

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
}