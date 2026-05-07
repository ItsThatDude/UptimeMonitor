namespace UptimeMonitor.Core.Domain.Monitors
{
    public class MonitorEvent
    {
        public int Id { get; set; }

        public required int MonitorId { get; set; }
        public MonitorConfiguration Monitor { get; set; }

        public required DateTime Timestamp { get; set; }
        public required string EventType { get; set; }
        public required string EventMessage { get; set; }
    }
}
