namespace UptimeMonitor.Core.Plugins.Monitoring
{
    public class MonitorResult : IMonitorResult
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public TimeSpan ResponseTime { get; set; }

        public MonitorTlsDetails? TlsDetails { get; set; }
    }
}
