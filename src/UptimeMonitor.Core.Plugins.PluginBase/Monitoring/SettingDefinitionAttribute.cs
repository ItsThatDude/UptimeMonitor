namespace UptimeMonitor.Core.Plugins.PluginBase.Monitoring
{
    public enum SettingDataType
    {
        String,
        MultilineString,
        Int,
        Boolean
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class SettingDefinitionAttribute : Attribute
    {
        private string displayName { get; set; }
        private SettingDataType dataType { get; set; }
        private bool required { get; set; }
        private bool allowMultiple { get; set; }
        private string description { get; set; }

        public SettingDefinitionAttribute(string displayName, SettingDataType dataType, bool required = false, bool allowMultiple = false, string description = "")
        {
            this.displayName = displayName;
            this.dataType = dataType;
            this.required = required;
            this.allowMultiple = allowMultiple;
            this.description = description;
        }

        public virtual string DisplayName { get { return displayName; } }
        public virtual SettingDataType DataType { get { return dataType; } }
        public virtual bool Required { get { return required; } }
        public virtual bool AllowMultiple { get { return allowMultiple; } }
        public virtual string Description { get { return description; } }
    }
}
