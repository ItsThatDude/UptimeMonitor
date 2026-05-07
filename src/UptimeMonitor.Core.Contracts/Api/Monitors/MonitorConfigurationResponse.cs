namespace UptimeMonitor.Core.Contracts.Api.Monitors
{
    public class MonitorConfigurationResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Target { get; set; }
        public bool Enabled { get; set; }
        public int Interval { get; set; }
        public int Timeout { get; set; }
        public int WarningThreshold { get; set; }
        public string Settings { get; set; } = "";

        public MonitorTlsDetailsDto? TlsDetails { get; set; }
    }
}
