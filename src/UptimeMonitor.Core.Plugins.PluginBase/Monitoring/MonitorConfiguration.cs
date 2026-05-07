namespace UptimeMonitor.Core.Plugins.PluginBase.Monitoring
{
    public class MonitorConfiguration
    {
        public required string Target { get; set; }

        public required int Timeout { get; set; }

        public required string Settings { get; set; }
    }
}
