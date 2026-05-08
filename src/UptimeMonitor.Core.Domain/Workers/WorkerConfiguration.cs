namespace UptimeMonitor.Core.Domain.Workers
{
    public class WorkerConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; } = "";
        public string Secret { get; set; } = "";

        public int ConfigUpdateInterval { get; set; } = 60;
        public DateTime LastCheckIn { get; set; } = DateTime.UtcNow;

        public bool Internal { get; set; } = false;
        public bool Approved { get; set; } = false;
    }
}
