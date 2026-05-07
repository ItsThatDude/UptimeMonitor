namespace UptimeMonitor.Service.MonitorService.Settings
{
    public class AuthOptions
    {
        public string ApiBaseUrl { get; set; } = string.Empty;
        public string WorkerId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
    }
}
