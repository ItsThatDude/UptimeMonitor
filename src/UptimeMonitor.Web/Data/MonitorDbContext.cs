using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Domain.Monitors;
using UptimeMonitor.Core.Domain.StatusPages;
using UptimeMonitor.Core.Domain.System;
using UptimeMonitor.Core.Domain.Workers;
using UptimeMonitor.Web.Data.ModelBuilders;

namespace UptimeMonitor.Web.Api.Data
{
    public class MonitorDbContext : DbContext
    {
        public DbSet<WorkerConfiguration> WorkerConfigurations { get; set; }

        public DbSet<MonitorConfiguration> MonitorConfigurations { get; set; }
        public DbSet<MonitorResultEntry> MonitorResults { get; set; }
        public DbSet<MonitorEvent> MonitorEvents { get; set; }

        public DbSet<StatusPage> StatusPages { get; set; }

        public DbSet<SystemSetting> Settings { get; set; }

        public MonitorDbContext(DbContextOptions<MonitorDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            WorkerConfigurationBuilder.Build(modelBuilder.Entity<WorkerConfiguration>());
            MonitorConfigurationBuilder.Build(modelBuilder.Entity<MonitorConfiguration>());
            MonitorResultBuilder.Build(modelBuilder.Entity<MonitorResultEntry>());
            MonitorEventBuilder.Build(modelBuilder.Entity<MonitorEvent>());
            StatusPageBuilder.Build(modelBuilder.Entity<StatusPage>());
            StatusPageMonitorGroupBuilder.Build(modelBuilder.Entity<StatusPageMonitorGroup>());
            StatusPageMonitorGroupMonitorBuilder.Build(modelBuilder.Entity<StatusPageMonitorGroupMonitor>());
            SystemSettingBuilder.Build(modelBuilder.Entity<SystemSetting>());

            base.OnModelCreating(modelBuilder);
        }
    }
}
