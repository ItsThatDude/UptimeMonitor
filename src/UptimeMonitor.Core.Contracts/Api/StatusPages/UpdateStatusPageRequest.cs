using System.ComponentModel.DataAnnotations;

namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class UpdateStatusPageRequest
    {
        [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
        public required string Slug { get; set; }

        public required string Name { get; set; }

        public List<UpdateMonitorGroupDto> MonitorGroups { get; set; } = [];
    }
}
