namespace UptimeMonitor.Core.Contracts.Api.Monitors
{
    public class UploadMonitorResultResponse
    {
        public int MonitorId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; }
        public string Message { get; set; } = "";
        public TimeSpan ResponseTime { get; set; }
        public MonitorTlsDetailsDto? TlsDetails { get; set; }
    }
}
