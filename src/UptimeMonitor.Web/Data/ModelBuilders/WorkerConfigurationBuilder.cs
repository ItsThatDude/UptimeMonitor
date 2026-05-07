using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Core.Domain.Workers;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class WorkerConfigurationBuilder
    {
        public static void Build(EntityTypeBuilder<WorkerConfiguration> entity)
        {
            entity.HasIndex(wc => wc.Name)
                .IsUnique();

            entity.Property(wc => wc.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(wc => wc.ConfigUpdateInterval)
                .HasDefaultValue(60);

            entity.Property(wc => wc.Secret)
                .IsRequired();

            entity.HasMany<MonitorResultEntry>()
                .WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
