namespace UptimeMonitor.Core.Contracts.Api.Monitors
{
    public class MonitorSimpleDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Type { get; set; }

        public required string Target { get; set; }

        public bool Enabled { get; set; }
    }
}
