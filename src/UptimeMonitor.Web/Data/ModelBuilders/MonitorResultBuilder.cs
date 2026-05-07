using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class MonitorResultBuilder
    {
        public static void Build(EntityTypeBuilder<MonitorResultEntry> entity)
        {
            entity.HasIndex(mr => new { mr.Timestamp, mr.MonitorConfigurationId, mr.WorkerConfigurationId });

            entity.Property(p => p.Timestamp)
                .HasConversion
                (
                    src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                    dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc)
                );
        }
    }
}
