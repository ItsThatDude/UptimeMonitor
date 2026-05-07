using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Core.Domain.StatusPages;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class StatusPageBuilder
    {
        public static void Build(EntityTypeBuilder<StatusPage> entity)
        {
            entity.Property(x => x.Slug)
                .IsRequired();

            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasMany(x => x.MonitorGroups)
                .WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
