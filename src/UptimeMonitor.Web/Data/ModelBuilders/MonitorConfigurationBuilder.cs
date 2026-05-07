using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Core.Domain.Monitors;
using UptimeMonitor.Core.Domain.StatusPages;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class MonitorConfigurationBuilder
    {
        public static void Build(EntityTypeBuilder<MonitorConfiguration> entity)
        {
            entity.Property(mc => mc.Enabled)
                .HasDefaultValue(true);

            entity.Property(mc => mc.Interval)
                .HasDefaultValue(60);

            entity.Property(mc => mc.Timeout)
                .HasDefaultValue(10);

            entity.Property(mc => mc.WarningThreshold)
                .HasDefaultValue(0);

            entity.Property(x => x.Settings)
                .HasColumnType("jsonb");

            entity.HasMany<MonitorResultEntry>()
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany<StatusPageMonitorGroupMonitor>()
                .WithOne(x => x.Monitor)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.TlsDetails)
                .WithOne().HasForeignKey<MonitorTlsDetails>()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
