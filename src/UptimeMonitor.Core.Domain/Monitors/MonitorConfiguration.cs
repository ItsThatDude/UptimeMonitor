namespace UptimeMonitor.Core.Domain.Monitors
{
    public class MonitorConfiguration
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Target { get; set; }
        public bool Enabled { get; set; } = true;
        public int Interval { get; set; } = 60;
        public int Timeout { get; set; } = 10;
        public int WarningThreshold { get; set; } = 0;
        public required string Settings { get; set; } = string.Empty;

        public MonitorTlsDetails? TlsDetails { get; set; }
    }
}
