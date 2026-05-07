using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Core.Domain.System;

namespace UptimeMonitor.Web.Data.ModelBuilders
{
    public static class SystemSettingBuilder
    {
        public static void Build(EntityTypeBuilder<SystemSetting> entity)
        {
            entity.Property(x => x.DisplayName)
                .IsRequired();

            entity.Property(x => x.Description)
                .IsRequired()
                .HasDefaultValue("");

            entity.Property(x => x.Value)
                .IsRequired();
        }
    }
}
