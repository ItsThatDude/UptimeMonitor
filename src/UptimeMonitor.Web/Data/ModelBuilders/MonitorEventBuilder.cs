using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Core.Domain.Monitors;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class MonitorEventBuilder
    {
        public static void Build(EntityTypeBuilder<MonitorEvent> entity)
        {
            entity.HasOne(m => m.Monitor)
                .WithMany().OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Timestamp)
                .HasConversion
                (
                    src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                    dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc)
                ).IsRequired();

            entity.Property(p => p.EventType)
                .IsRequired();

            entity.Property(p => p.EventMessage)
                .IsRequired();
        }
    }
}
