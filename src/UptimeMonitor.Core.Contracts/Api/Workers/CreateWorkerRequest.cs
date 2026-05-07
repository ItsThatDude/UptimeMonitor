namespace UptimeMonitor.Core.Contracts.Api.Workers
{
    public class CreateWorkerRequest
    {
        public required string Name { get; set; }
        public required string Location { get; set; }

        public required string Secret { get; set; }

        public int ConfigUpdateInterval { get; set; }
    }
}
