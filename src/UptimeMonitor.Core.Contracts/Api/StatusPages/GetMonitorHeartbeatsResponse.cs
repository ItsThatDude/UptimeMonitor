namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class GetMonitorHeartbeatsResponse
    {
        public int MonitorId { get; set; }
        public double Uptime { get; set; }
        public List<HeartbeatDto> Heartbeats { get; set; } = [];
    }
}
