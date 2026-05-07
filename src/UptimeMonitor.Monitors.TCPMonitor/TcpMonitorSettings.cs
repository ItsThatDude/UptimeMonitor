using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Monitors.TCPMonitor
{
    public class TcpMonitorSettings : IMonitorSettings<TcpMonitor>
    {
        [SettingDefinition(
            displayName: "Use SSL",
            dataType: SettingDataType.Boolean,
            required: false,
            allowMultiple: false,
            description: "Connect over TCP using SSL."
        )]
        public bool UseSSL { get; set; } = false;

        [SettingDefinition(
            displayName: "CA Certificate",
            dataType: SettingDataType.MultilineString,
            required: false,
            allowMultiple: false,
            description: "The CA Certificate used to validate the SSL Certificate."
        )]
        public string? CACertificate { get; set; } = null;

        [SettingDefinition(
            displayName: "Ignore Invalid Certificates",
            dataType: SettingDataType.Boolean,
            required: false,
            allowMultiple: false,
            description: "Accept invalid certificates."
        )]
        public bool IgnoreInvalidCertificates { get; set; } = false;
    }
}
