namespace UptimeMonitor.Core.Contracts.Api.Monitors
{
    public class GetMonitorStatisticsResponse
    {
        public int TotalMonitors { get; set; }
        public int EnabledMonitors { get; set; }
        public int DisabledMonitors { get; set; }
        public int UpMonitors { get; set; }
        public int DownMonitors { get; set; }
    }
}
