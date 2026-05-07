namespace UptimeMonitor.Core.Contracts.Api.Workers
{
    public class WorkerAuthenticationRequest
    {
        public required string WorkerId { get; set; }
        public required string Secret { get; set; }
    }
}
