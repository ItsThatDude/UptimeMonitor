namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class CreateStatusPageMonitorGroup
    {
        public required string Name { get; set; }

        public List<int> MonitorIds { get; set; } = [];
    }
}
