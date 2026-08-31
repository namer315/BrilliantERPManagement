using BrilliantWhatsAppWeb.Components;
using CommonData.Session;
using WhatsAppData.VO.WhatsApp;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Load the WhatsAppData assembly BEFORE NHibernate builds its persistence model.
        // MappingCompiler scans already-loaded assemblies; without this, WhatsAppData isn't
        // loaded yet and WhatsAppTenantMap is skipped → "WhatsAppTenantVO is not mapped".
        _ = typeof(WhatsAppTenantVO).Assembly;
        
        //connect to the database before starting the application
        await Connection.DataBaseConnect();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error" , createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/not-found" , createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}