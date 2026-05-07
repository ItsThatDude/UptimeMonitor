namespace UptimeMonitor.Core.Domain.StatusPages
{
    public class StatusPageMonitorGroup
    {
        public int Id { get; set; }

        public int SortOrder { get; set; }

        public required string Name { get; set; }

        public List<StatusPageMonitorGroupMonitor> Monitors { get; } = new();
    }
}
