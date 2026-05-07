using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Core.Domain.StatusPages;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class StatusPageMonitorGroupBuilder
    {
        public static void Build(EntityTypeBuilder<StatusPageMonitorGroup> entity)
        {
            entity.Property(x => x.Name)
                .IsRequired();

            entity.HasMany(x => x.Monitors)
                .WithOne(x => x.MonitorGroup).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
