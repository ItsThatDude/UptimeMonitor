namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class StatusPageConfigResponse
    {
        public int Id { get; set; }

        public required string Slug { get; set; }

        public required string Name { get; set; }

        public List<StatusPageGroupConfigDto> MonitorGroups { get; } = new();
    }
}
