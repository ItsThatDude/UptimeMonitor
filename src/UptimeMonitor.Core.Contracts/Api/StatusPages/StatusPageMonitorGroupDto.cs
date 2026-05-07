namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class StatusPageMonitorGroupDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public List<StatusPageMonitorDto> Monitors { get; set; } = [];
    }
}
