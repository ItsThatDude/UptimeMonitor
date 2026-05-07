namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class GetStatusPageResponse
    {
        public required string Name { get; set; }

        public List<StatusPageMonitorGroupDto> MonitorGroups { get; set; } = [];
    }
}
