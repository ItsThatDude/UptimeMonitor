using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.Workers;
using UptimeMonitor.Core.Plugins;
using UptimeMonitor.Core.Plugins.Monitoring;

namespace UptimeMonitor.Service.MonitorService.Monitor
{
    public class MonitoringBackgroundService : BackgroundService
    {
        private readonly ILogger<MonitoringBackgroundService> _logger;

        private readonly IServiceProvider _serviceProvider;
        private readonly PluginManager _pluginManager;

        private WorkerConfiguration? _configuration;

        private List<MonitorInstance> _instances = new();

        public MonitoringBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MonitoringBackgroundService> logger,
            PluginManager pluginManager)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _pluginManager = pluginManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting monitoring background service");
            using (var scope = _serviceProvider.CreateScope())
            {
                var _workerService = scope.ServiceProvider.GetRequiredService<IWorkerService>();
                
                while(!_workerService.IsRegistered) {
                    try
                    {
                        await _workerService.WaitForRegistrationAsync(stoppingToken);
                    }
                    catch(SocketException ex)
                    {
                        _logger.LogWarning(ex, "An exception occurred attempting to register/authenticate the worker: {message}", ex.Message);
                    }
                    finally
                    {
                        if(!_workerService.IsRegistered)
                        {
                            _logger.LogWarning("Failed to register worker, sleeping for 30 seconds");
                            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                        }
                        else if(!_workerService.IsAuthenticated)
                        {
                            _logger.LogWarning("Failed to authenticate, sleeping for 30 seconds");
                            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                        }
                    }
                }

                try {
                    var types = _pluginManager.GetMonitorTypes();

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Fetching worker configuration...");
                        _configuration = await _workerService.GetConfigurationAsync(stoppingToken);

                        if (_configuration == null)
                        {
                            throw new Exception("Failed to fetch worker configuration");
                        }

                        _logger.LogInformation("Fetching monitor configurations...");
                        List<MonitorConfigurationResponse> monitorConfigs = await _workerService.GetMonitorConfigsAsync(stoppingToken);

                        foreach (var monitorConfig in monitorConfigs)
                        {
                            var instance = _instances.FirstOrDefault(i => i.Id == monitorConfig.Id);
                            if (instance == null)
                            {
                                _logger.LogInformation("Creating new instance of Monitor {monitorName} (id: {monitorId})...", monitorConfig.Name, monitorConfig.Id);

                                var monitorType = types.Where(t => t.Name == monitorConfig.Type && t.GetInterfaces().Contains(typeof(IMonitorType)) && !t.IsAbstract && !t.IsInterface)
                                    .FirstOrDefault();

                                if (monitorType == null)
                                {
                                    _logger.LogError("Monitor Type {typeName} not found", monitorConfig.Type);
                                    continue;
                                }

                                IMonitorType? monitor = Activator.CreateInstance(monitorType) as IMonitorType;

                                if (monitor == null)
                                {
                                    _logger.LogError("Failed to activate monitor of Type {typeName}", monitorConfig.Type);
                                    continue;
                                }

                                monitor.ConfigureServices(_serviceProvider);

                                monitor.UpdateConfiguration(new Core.Plugins.PluginBase.Monitoring.MonitorConfiguration
                                {
                                    Target = monitorConfig.Target,
                                    Timeout = monitorConfig.Timeout,
                                    Settings = monitorConfig.Settings
                                });

                                instance = new MonitorInstance(_logger, _workerService, monitor, monitorConfig, _configuration);
                                instance.Run(stoppingToken);
                                _instances.Add(instance);
                            }
                            else
                            {
                                instance.UpdateConfiguration(monitorConfig);
                            }
                        }

                        var instancesToRemove = _instances
                            .Where(i => !monitorConfigs.Any(mc => i.Id == mc.Id))
                            .ToArray();

                        foreach (var instanceToRemove in instancesToRemove)
                        {
                            _logger.LogInformation("Monitor {monitorName} (id: {monitorId}) no longer exists, removing running instance...", instanceToRemove.Name, instanceToRemove.Id);

                            instanceToRemove.Stop();
                            instanceToRemove.Dispose();

                            _instances.Remove(instanceToRemove);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(_configuration.ConfigUpdateInterval), stoppingToken);
                    }

                    // Cancellation requested
                    foreach(var instance in _instances)
                    {
                        instance.Stop();
                        instance.Dispose();
                    }
                }
                catch (Exception ex) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "An error occurred: {errorMessage}", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "A critical error occurred: {errorMessage}", ex.Message);
                }
            }
        }
    }
}
