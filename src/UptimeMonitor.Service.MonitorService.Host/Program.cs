using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using UptimeMonitor.Core.Plugins;
using UptimeMonitor.Service.MonitorService.Extensions;
using UptimeMonitor.Service.MonitorService.HealthChecks;
using UptimeMonitor.Service.MonitorService.Monitor;
using UptimeMonitor.Service.MonitorService.Settings;

namespace UptimeMonitor.Service.MonitorService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);
            ConfigureServices(builder);
            
            var app = builder.Build();
            ConfigureMiddleware(app);

            app.Run();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .CreateLogger();

            builder.Services.AddSerilog();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddPluginLoader();
            builder.Services.AddHttpClient();

            builder.Services.Configure<AuthOptions>(options =>
                builder.Configuration.GetSection("Auth").Bind(options));
            builder.Services.AddScoped<IWorkerService, WorkerApiService>();
            
            builder.Services.AddMonitorService();

            builder.Services.AddHealthChecks()
                .AddCheck<BackendHealthCheck>("backend_health_check");
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.MapHealthChecks("/healthz");
        }
    }
}
