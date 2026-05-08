using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.StatusPages;
using UptimeMonitor.Web.Api.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UptimeMonitor.Web.Api.Services
{
    public class HeartbeatService
    {
        private readonly MonitorDbContext _dbContext;

        public HeartbeatService(MonitorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetMonitorHeartbeatsResponse?> GetHeartbeatsAsync(int monitorId, TimeSpan aggregateInterval, int limit, string? location = null, string? worker = null)
        {
            return (await GetHeartbeatsAsync([monitorId], aggregateInterval, limit, location, worker)).FirstOrDefault();
        }

        public async Task<IEnumerable<GetMonitorHeartbeatsResponse>> GetHeartbeatsAsync(
            IEnumerable<int> monitorIds, TimeSpan aggregateInterval, int limit, string? location = null, string? worker = null)
        {
            var now = DateTime.UtcNow;

            // Generate the expected time periods (descending order)
            var expectedIntervals = Enumerable.Range(0, limit)
                .Select(i => now - TimeSpan.FromTicks(i * aggregateInterval.Ticks))
                .Select(t => new DateTime(t.Ticks / aggregateInterval.Ticks * aggregateInterval.Ticks, DateTimeKind.Utc)) // Align to interval
                .ToList();

            var heartbeatLookupQuery = _dbContext.MonitorResults
                .Where(x => monitorIds.Contains(x.MonitorConfigurationId));

            if (worker != null)
            {
                var selectedWorker = await _dbContext.WorkerConfigurations
                    .Where(x => x.Name == worker)
                    .FirstOrDefaultAsync();

                if (selectedWorker != null)
                {
                    heartbeatLookupQuery = heartbeatLookupQuery.Where(x => x.WorkerConfigurationId == selectedWorker.Id);
                }
            }
            else if (location != null)
            {
                var workerIds = await _dbContext.WorkerConfigurations
                    .Where(x => x.Location == location)
                    .Select(w => w.Id).ToListAsync();

                heartbeatLookupQuery = heartbeatLookupQuery.Where(x => workerIds.Contains(x.WorkerConfigurationId));
            }

            // Fetch heartbeats from DB and group by monitor ID
            var heartbeatLookup = await heartbeatLookupQuery.Where(x => x.Timestamp >= now - TimeSpan.FromTicks(limit * aggregateInterval.Ticks))
                .GroupBy(x => x.MonitorConfigurationId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            // Ensure every monitorId is included in the result
            var mapped = monitorIds.Select(monitorId =>
            {
                var monitorHeartbeats = heartbeatLookup.TryGetValue(monitorId, out var heartbeatsList)
                    ? heartbeatsList
                    : new List<MonitorResultEntry>(); // Ensure an empty list if no heartbeats exist

                // Group heartbeats into predefined intervals
                var groupedHeartbeats = monitorHeartbeats
                    .GroupBy(hb => new DateTime(hb.Timestamp.Ticks / aggregateInterval.Ticks * aggregateInterval.Ticks, DateTimeKind.Utc))
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Fill missing intervals
                var heartbeats = expectedIntervals
                    .Select(interval => new HeartbeatDto
                    {
                        Timestamp = interval,
                        ResponseTime = groupedHeartbeats.TryGetValue(interval, out var hbList) && hbList.Any()
                            ? hbList.Average(x => x.ResponseTime.TotalMilliseconds)
                            : 0, // Default response time if no data
                        Up = groupedHeartbeats.TryGetValue(interval, out hbList) && hbList.Any()
                            ? hbList.GroupBy(x => x.IsSuccessful).OrderByDescending(g => g.Count()).Select(g => g.Key).First()
                            : null, // Default to 'down' if no data
                        TotalHeartbeats = groupedHeartbeats.TryGetValue(interval, out hbList) && hbList.Any()
                            ? hbList.Count : 0,
                        UpHeartbeats = groupedHeartbeats.TryGetValue(interval, out hbList) && hbList.Any()
                            ? hbList.Where(x => x.IsSuccessful).Count() : 0
                    })
                    .OrderByDescending(hb => hb.Timestamp)
                    .ToList();

                double totalHeartbeats = heartbeats.Sum(hb => hb.TotalHeartbeats);
                double totalUpHeartbeats = heartbeats.Sum(hb => hb.UpHeartbeats);
                double uptime = 0;

                if (totalHeartbeats > 0 && totalUpHeartbeats > 0)
                {
                    uptime = Math.Round(totalUpHeartbeats / totalHeartbeats * 100, 2);
                }

                return new GetMonitorHeartbeatsResponse
                {
                    MonitorId = monitorId,
                    Uptime = uptime,
                    Heartbeats = heartbeats
                };
            });

            return mapped;
        }
    }
}
