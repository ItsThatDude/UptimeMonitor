namespace UptimeMonitor.Core.Contracts.Api.Monitors
{
    public class MonitorEventDto
    {
        public int Id { get; set; }

        public required string MonitorName { get; set; }
        public required string MonitorType { get; set; }

        public DateTime Timestamp { get; set; }

        public required string EventType { get; set; }
        public required string EventMessage { get; set; }
    }
}
