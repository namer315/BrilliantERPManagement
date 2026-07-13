using FastEndpoints;
using FastEndpoints.Swagger;
using BrilliantWhatsAppAPI.Processors;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddFastEndpoints();
        builder.Services.SwaggerDocument();


        var app = builder.Build();

        //app.UseFastEndpoints();
        app.UseSwaggerGen();

        app.UseFastEndpoints(c =>
        {
            //c.Endpoints.PreProcessors.Add(new TokenPreProcessor());
            c.Endpoints.Configurator = ep =>
            {
                ep.PreProcessor<TokenPreProcessor>(Order.Before);
                //ep.PostProcessor<ExceptionHandler>(Order.After);
            };
            c.Endpoints.RoutePrefix = "WhatsAppAPI";
        });
        app.MapGet("/" , async context =>
        {
            context.Response.Redirect("/swagger");
            await Task.CompletedTask;
        });

        //app.MapGet("/" , () => "Hello World!");

        app.Run();
    }
}