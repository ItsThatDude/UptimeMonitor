using Microsoft.Extensions.Logging;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.Workers;
using UptimeMonitor.Core.Plugins.Monitoring;

namespace UptimeMonitor.Service.MonitorService.Monitor
{
    public class MonitorInstance : IDisposable
    {
        public int Id => _configuration.Id;
        public string Name => _configuration.Name;

        private readonly ILogger _logger;
        private readonly IWorkerService _workerService;

        private CancellationToken _stoppingToken { get; set; }
        private IMonitorType _monitor { get; set; }
        private MonitorConfigurationResponse _configuration { get; set; }

        private bool _running = false;

        private Task? _task;

        public MonitorInstance(
            ILogger logger,
            IWorkerService monitoringClient,
            IMonitorType monitor,
            MonitorConfigurationResponse monitorConfiguration,
            WorkerConfiguration workerConfiguration)
        {
            _logger = logger;
            _workerService = monitoringClient;
            _monitor = monitor;
            _configuration = monitorConfiguration;
        }

        public void UpdateConfiguration(MonitorConfigurationResponse configuration)
        {
            _configuration = configuration;
            _monitor.UpdateConfiguration(new Core.Plugins.PluginBase.Monitoring.MonitorConfiguration
            {
                Target = configuration.Target,
                Timeout = configuration.Timeout,
                Settings = configuration.Settings
            });
        }

        private async Task RunThread(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _running)
            {
                var timestamp = DateTime.UtcNow;

                try
                {
                    var result = await _monitor.CheckAsync(cancellationToken);

                    _logger.LogInformation("[{monitor}] Target: {target} Success: {success}, Message: {message}", _monitor.Name, _configuration.Target, result.IsSuccessful, result.Message);

                    await _workerService.ReportResultAsync(_configuration.Id, result.IsSuccessful, result.Message, timestamp, result.ResponseTime, result.TlsDetails, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{monitor}] Error while checking target {target}: {error}", _monitor.Name, _configuration.Target, ex.Message);
                }

                var timespan = timestamp.AddSeconds(_configuration.Interval) - DateTime.UtcNow;
                await Task.Delay(timespan, _stoppingToken);
            }
        }

        public void Run(CancellationToken cancellationToken)
        {
            _running = true;

            _task = RunThread(cancellationToken);
        }

        public void Stop()
        {
            _logger.LogInformation("Stopping monitor {monitor}", _monitor.Name);

            _running = false;

            if (_task != null && _task.Status == TaskStatus.Running)
            {
                try
                {
                    _task.Wait();
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "An exception occurred while waiting for monitor to stop: {message}", ex.Message);
                }
            }

            _logger.LogInformation("Monitor {monitor} stopped", _monitor.Name);
        }

        public void Dispose()
        {
            if (_monitor is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
