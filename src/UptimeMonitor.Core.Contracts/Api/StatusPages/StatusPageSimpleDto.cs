namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class StatusPageSimpleDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public bool Default { get; set; }
    }
}
