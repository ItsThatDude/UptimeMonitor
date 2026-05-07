namespace UptimeMonitor.Core.Contracts.Api.Workers
{
    public class WorkerRegistrationRequest
    {
        public required string WorkerId { get; set; }
        public required string Location { get; set; }
    }
}
