namespace UptimeMonitor.Core.Domain.StatusPages
{
    public class StatusPage
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public required string Slug { get; set; }

        public bool Default { get; set; } = false;

        public List<StatusPageMonitorGroup> MonitorGroups { get; } = new();
    }
}
