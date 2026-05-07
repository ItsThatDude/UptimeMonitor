using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Domain.Monitors;

namespace UptimeMonitor.Web.Models.Mappings
{
    public static class MonitorMappings
    {
        public static MonitorSimpleDto MapSimpleDto(MonitorConfiguration monitor)
        {
            return new MonitorSimpleDto
            {
                Id = monitor.Id,
                Name = monitor.Name,
                Type = monitor.Type,
                Target = monitor.Target
            };
        }
    }
}