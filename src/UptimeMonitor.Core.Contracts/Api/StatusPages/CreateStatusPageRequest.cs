using System.ComponentModel.DataAnnotations;

namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class CreateStatusPageRequest
    {
        [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
        public required string Slug { get; set; }

        public required string Name { get; set; }

        public List<CreateStatusPageMonitorGroup> MonitorGroups { get; set; } = [];
    }
}
