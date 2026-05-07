namespace UptimeMonitor.Core.Domain.Monitors
{
    public class MonitorTlsDetails
    {
        public int Id { get; set; }

        public required string Subject { get; set; }
        public required string Issuer { get; set; }

        public required DateTime NotBefore { get; set; }
        public required DateTime NotAfter { get; set; }
    }
}
