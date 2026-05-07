using UptimeMonitor.Core.Contracts.Api.Monitors;

namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class StatusPageGroupConfigDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public List<MonitorSimpleDto> Monitors { get; } = new();
    }
}
