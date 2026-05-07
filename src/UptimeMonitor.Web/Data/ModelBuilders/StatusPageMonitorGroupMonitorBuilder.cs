using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Core.Domain.StatusPages;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class StatusPageMonitorGroupMonitorBuilder
    {
        public static void Build(EntityTypeBuilder<StatusPageMonitorGroupMonitor> entity)
        {
            entity.HasKey(x => new { x.MonitorGroupId, x.MonitorId });
        }
    }
}
