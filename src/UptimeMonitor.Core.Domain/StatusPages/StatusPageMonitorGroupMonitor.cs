using UptimeMonitor.Core.Domain.Monitors;

namespace UptimeMonitor.Core.Domain.StatusPages
{
    public class StatusPageMonitorGroupMonitor
    {
        public int MonitorId { get; set; }
        public int MonitorGroupId { get; set; }
        public MonitorConfiguration Monitor { get; set; } = null!;
        public StatusPageMonitorGroup MonitorGroup { get; set; } = null!;

        public int SortOrder { get; set; }
    }
}
