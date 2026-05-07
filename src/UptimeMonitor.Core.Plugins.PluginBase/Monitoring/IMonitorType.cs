using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Core.Plugins.Monitoring
{
    public interface IMonitorType
    {
        public string Name { get; }
        public string DisplayName { get; }

        Task<MonitorResult> CheckAsync(CancellationToken cancellationToken);

        void UpdateConfiguration(MonitorConfiguration configuration);
        void ConfigureServices(IServiceProvider serviceProvider);
    }
}