using Microsoft.Extensions.Hosting;

namespace UptimeMonitor.Core.Plugins.Services
{
    public class PluginLoaderService : BackgroundService
    {
        private readonly PluginManager _pluginManager;

        public PluginLoaderService(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _pluginManager.LoadPlugins();

            return Task.CompletedTask;
        }
    }
}
