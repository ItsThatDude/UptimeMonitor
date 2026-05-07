namespace UptimeMonitor.Core.Contracts.Api.Workers
{
    public class WorkerTokenResponse
    {
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
