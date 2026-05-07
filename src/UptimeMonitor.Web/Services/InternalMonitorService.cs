using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.Workers;
using UptimeMonitor.Core.Domain.Monitors;
using UptimeMonitor.Service.MonitorService.Monitor;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Services
{
    public class InternalMonitorService : IWorkerService
    {
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<MonitorDbContext> _dbContextFactory;

        public bool IsAuthenticated => true;

        public bool IsRegistered { get; private set; } = false;

        public InternalMonitorService(IMapper mapper, IDbContextFactory<MonitorDbContext> dbContextFactory)
        {
            _mapper = mapper;
            _dbContextFactory = dbContextFactory;    
        }

        public async Task<WorkerConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var config = await dbContext.WorkerConfigurations.FirstOrDefaultAsync(w => w.Internal == true, cancellationToken);

            if (config == null)
            {
                throw new Exception("Worker not found");
            }
            
            config.LastCheckIn = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<WorkerConfiguration>(config);
        }

        public async Task<List<MonitorConfigurationResponse>> GetMonitorConfigsAsync(CancellationToken cancellationToken)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var monitors = await dbContext.MonitorConfigurations
                .Where(mc => mc.Enabled == true).ToListAsync(cancellationToken);

            return monitors.Select(_mapper.Map<MonitorConfigurationResponse>).ToList();
        }

        public async Task<bool> ReportResultAsync(int monitorId, bool success, string message, DateTime timestamp, TimeSpan responseTime, Core.Plugins.Monitoring.MonitorTlsDetails? tlsDetails = null, CancellationToken cancellationToken = default)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var worker = await dbContext.WorkerConfigurations.FirstOrDefaultAsync(w => w.Internal == true);

            if (worker == null)
            {
                throw new Exception("Worker could not be found");
            }

            var monitor = await dbContext.MonitorConfigurations
                .Include(m => m.TlsDetails).FirstOrDefaultAsync(m => m.Id == monitorId);

            if (monitor == null)
            {
                throw new Exception("Monitor could not be found");
            }

            if(tlsDetails != null)
            {
                if (monitor.TlsDetails == null)
                {
                    monitor.TlsDetails = new MonitorTlsDetails
                    {
                        Subject = tlsDetails.Subject,
                        Issuer = tlsDetails.Issuer,
                        NotBefore = tlsDetails.NotBefore,
                        NotAfter = tlsDetails.NotAfter
                    };
                }
                else
                {
                    if (monitor.TlsDetails.Subject != tlsDetails.Subject)
                    {
                        monitor.TlsDetails.Subject = tlsDetails.Subject;
                    }

                    if (monitor.TlsDetails.Issuer != tlsDetails.Issuer)
                    {
                        monitor.TlsDetails.Issuer = tlsDetails.Issuer;
                    }

                    if (monitor.TlsDetails.NotBefore != tlsDetails.NotBefore)
                    {
                        monitor.TlsDetails.NotBefore = tlsDetails.NotBefore;
                    }

                    if (monitor.TlsDetails.NotAfter != tlsDetails.NotAfter)
                    {
                        monitor.TlsDetails.NotAfter = tlsDetails.NotAfter;
                    }
                }
            }
            else
            {
                if(monitor.TlsDetails != null)
                {
                    monitor.TlsDetails = null;
                }
            }

            var lastResult = await dbContext.MonitorResults
                    .Where(mr => mr.MonitorConfigurationId == monitorId)
                    .OrderByDescending(mr => mr.Timestamp)
                    .FirstOrDefaultAsync(cancellationToken);

            if(lastResult != null)
            {
                if(lastResult.IsSuccessful && !success)
                {
                    await dbContext.MonitorEvents.AddAsync(new MonitorEvent
                    {
                        MonitorId = monitorId,
                        Timestamp = DateTime.UtcNow,
                        EventType = "MonitorOffline",
                        EventMessage = "Monitored target has gone offline"
                    }, cancellationToken);
                }
                else if(!lastResult.IsSuccessful && success)
                {
                    await dbContext.MonitorEvents.AddAsync(new MonitorEvent
                    {
                        MonitorId = monitorId,
                        Timestamp = DateTime.UtcNow,
                        EventType = "MonitorOnline",
                        EventMessage = "Monitored target is back online"
                    }, cancellationToken);
                }

                if(monitor.WarningThreshold > 0 && lastResult.IsSuccessful && success)
                {
                    if(lastResult.ResponseTime.TotalMilliseconds <= monitor.WarningThreshold
                        && responseTime.TotalMilliseconds > monitor.WarningThreshold)
                    {
                        await dbContext.MonitorEvents.AddAsync(new MonitorEvent
                        {
                            MonitorId = monitorId,
                            Timestamp = DateTime.UtcNow,
                            EventType = "MonitorLatencyThreshold",
                            EventMessage = $"Target's latency ({responseTime.TotalMilliseconds}ms) is higher than warning threshold of {monitor.WarningThreshold}ms"
                        }, cancellationToken);
                    }
                }
            }

            var dbItem = new MonitorResultEntry()
            {
                WorkerConfigurationId = worker.Id,
                MonitorConfigurationId = monitorId,
                Timestamp = timestamp,
                IsSuccessful = success,
                Message = message,
                ResponseTime = responseTime
            };

            await dbContext.MonitorResults.AddAsync(dbItem, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task WaitForRegistrationAsync(CancellationToken cancellationToken)
        {
            if(!IsRegistered) {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                var config = await dbContext.WorkerConfigurations.FirstOrDefaultAsync(w => w.Internal == true, cancellationToken);

                if (config == null)
                {
                    config = new Core.Domain.Workers.WorkerConfiguration
                    {
                        Name = "UptimeMonitor",
                        Location = "UptimeMonitor",
                        Secret = "",
                        Internal = true,
                        Approved = true
                    };

                    await dbContext.WorkerConfigurations.AddAsync(config, cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                IsRegistered = true;
            }
        }
    }
}