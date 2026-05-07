namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class UpdateMonitorGroupDto
    {
        public int? Id { get; set; }

        public required string Name { get; set; }

        public List<int> MonitorIds { get; set; } = [];
    }
}
