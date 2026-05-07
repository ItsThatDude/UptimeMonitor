using Microsoft.Extensions.DependencyInjection;
using UptimeMonitor.Core.Plugins.Services;

namespace UptimeMonitor.Core.Plugins
{
    public static class ServiceProviderExtensions
    {
        public static void AddPluginLoader(this IServiceCollection services)
        {
            services.AddSingleton<PluginManager>();
            services.AddHostedService<PluginLoaderService>();
        }
    }
}
