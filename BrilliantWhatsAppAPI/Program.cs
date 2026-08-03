using FastEndpoints;
using FastEndpoints.Swagger;
using BrilliantWhatsAppAPI.Processors;
using BrilliantWhatsAppAPI.Infrastructure;
using CommonData.Session;
using CommonData.DAO;

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

        var app = builder.Build();

        app.UseSwaggerGen();

        app.UseFastEndpoints(c =>
        {
            c.Endpoints.Configurator = ep =>
            {
                ep.PreProcessor<TokenPreProcessor>(Order.Before);
                ep.PreProcessor<TenantPreProcessor>(Order.Before);
                ep.PostProcessor<TenantPreProcessor>(Order.After);
                ep.PostProcessor<ExceptionHandler>(Order.After);
            };
            c.Endpoints.RoutePrefix = "WhatsAppAPI";
        });

        app.MapGet("/" , async context =>
        {
            context.Response.Redirect("/swagger");
            await Task.CompletedTask;
        });

        app.Run();
    }
}