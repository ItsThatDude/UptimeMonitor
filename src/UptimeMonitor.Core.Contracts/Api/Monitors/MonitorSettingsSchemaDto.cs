namespace UptimeMonitor.Core.Contracts.Api.Monitors
{
    public class MonitorSettingsSchemaDto
    {
        public required string MonitorType { get; set; }

        public IEnumerable<MonitorSettingsPropertyDto> Properties { get;set;} = [];
    }

    public class MonitorSettingsPropertyDto
    {
        public required string Key { get; set; }
        public required string DataType { get; set; }
        public required string DisplayName { get; set; }
        public string Description { get; set; } = "";
        public object? DefaultValue { get; set; }
        public bool Required { get; set; }
        public bool AllowMultiple { get; set; }
    }
}
