namespace UptimeMonitor.Core.Domain.System
{
    public class SystemSetting
    {
        public required string Id { get; set; }

        public required string DisplayName { get; set; }

        public required string Description { get; set; }

        public required string Value { get; set; }
    }
}
