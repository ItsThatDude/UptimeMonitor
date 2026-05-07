namespace UptimeMonitor.Core.Contracts.Api.Workers
{
    public class WorkerConfiguration
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public int ConfigUpdateInterval { get; set; }
        public DateTime LastCheckIn { get; set; }
    }
}
