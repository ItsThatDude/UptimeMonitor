using UptimeMonitor.Core.Plugins.Monitoring;

namespace UptimeMonitor.Web.Api.Data
{
    public class MonitorResultEntry : IMonitorResult
    {
        public int Id { get; set; }
        public required int WorkerConfigurationId { get; set; }
        public required int MonitorConfigurationId { get; set; }
        public required DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; }
        public string Message { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
    }
}
