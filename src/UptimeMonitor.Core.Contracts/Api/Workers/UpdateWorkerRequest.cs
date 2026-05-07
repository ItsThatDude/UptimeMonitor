namespace UptimeMonitor.Core.Contracts.Api.Workers
{
    public class UpdateWorkerRequest
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Secret { get; set; }

        public int ConfigUpdateInterval { get; set; }
    }
}
