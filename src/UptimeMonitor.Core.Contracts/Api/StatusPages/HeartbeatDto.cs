namespace UptimeMonitor.Core.Contracts.Api.StatusPages
{
    public class HeartbeatDto
    {
        public DateTime Timestamp { get; set; }

        public double ResponseTime { get; set; }

        public bool? Up { get; set; }

        public int TotalHeartbeats { get; set; }
        public int UpHeartbeats { get; set; }
    }
}
