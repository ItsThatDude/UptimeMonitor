
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Api.Services
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly ILogger<CleanupBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CleanupBackgroundService(ILogger<CleanupBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Cleanup Worker Service");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MonitorDbContext>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    var cutoffDate = DateTime.UtcNow.AddMonths(-1);

                    await CleanupHeartbeats(dbContext, cutoffDate);
                    await CleanupMonitorEvents(dbContext, cutoffDate);

                    await dbContext.SaveChangesAsync();

                    await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
                }
            }

            _logger.LogInformation("Stopping Cleanup Worker Service");
        }

        private async Task CleanupHeartbeats(MonitorDbContext dbContext, DateTime cutoffDate)
        {
            var results = await dbContext.MonitorResults.Where(r => r.Timestamp < cutoffDate)
                                    .ToListAsync();

            _logger.LogInformation($"Removing {results.Count} heartbeats from the database.");

            dbContext.MonitorResults.RemoveRange(results);
        }

        private async Task CleanupMonitorEvents(MonitorDbContext dbContext, DateTime cutoffDate)
        {
            var results = await dbContext.MonitorEvents.Where(r => r.Timestamp < cutoffDate)
                                   .ToListAsync();

            _logger.LogInformation($"Removing {results.Count} monitor events from the database.");

            dbContext.MonitorEvents.RemoveRange(results);
        }
    }
}
