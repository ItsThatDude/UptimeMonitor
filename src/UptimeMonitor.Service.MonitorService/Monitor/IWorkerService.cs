using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.Workers;
using UptimeMonitor.Core.Plugins.Monitoring;

namespace UptimeMonitor.Service.MonitorService.Monitor
{
    public interface IWorkerService
    {
        bool IsRegistered { get; }
        bool IsAuthenticated { get; }

        Task WaitForRegistrationAsync(CancellationToken cancellationToken);
        Task<WorkerConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken);
        Task<List<MonitorConfigurationResponse>> GetMonitorConfigsAsync(CancellationToken cancellationToken);
        Task<bool> ReportResultAsync(int monitorId, bool success, string message, DateTime timestamp, TimeSpan responseTime, MonitorTlsDetails? tlsDetails = null, CancellationToken cancellationToken = default);
    }
}
