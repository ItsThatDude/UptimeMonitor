namespace UptimeMonitor.Core.Contracts.Api.Monitors
{
    public class UpdateMonitorConfigRequest
    {
        public required string Name { get; set; }
        public required string Target { get; set; }
        public required bool Enabled { get; set; }
        public required int Interval { get; set; }
        public required int Timeout { get; set; }
        public required int WarningThreshold { get; set; }
        public required string Settings { get; set; }
    }
}
