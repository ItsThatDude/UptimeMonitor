using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Core.Plugins
{
    public class SettingDefinition
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required SettingDataType DataType { get; set; }
        public bool Required { get; set; } = false;
        public string Description { get; set; } = "";
        public bool AllowMultiple { get; internal set; }
        public object? DefaultValue { get; internal set; }
    }
}
