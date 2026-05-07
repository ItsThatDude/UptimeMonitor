using Microsoft.Extensions.DependencyInjection;
using UptimeMonitor.Service.MonitorService.Monitor;

namespace UptimeMonitor.Service.MonitorService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMonitorService(this IServiceCollection services)
        {
            services.AddHostedService<MonitoringBackgroundService>();
        }
    }
}