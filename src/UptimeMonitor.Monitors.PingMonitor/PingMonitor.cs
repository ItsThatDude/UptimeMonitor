using System.Diagnostics;
using System.Net.NetworkInformation;
using UptimeMonitor.Core.Plugins.Monitoring;
using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Monitors.PingMonitor
{
    public class PingMonitor : IMonitorType
    {
        public string Name => nameof(PingMonitor);
        public string DisplayName => "Ping Monitor";

        private MonitorConfiguration? _configuration;

        public void UpdateConfiguration(MonitorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceProvider serviceProvider)
        {
            // No external services are needed for this monitor
        }

        public async Task<MonitorResult> CheckAsync(CancellationToken cancellationToken)
        {
            if (_configuration == null)
            {
                throw new Exception("Monitor has not been configured");
            }

            var stopwatch = Stopwatch.StartNew();

            using (Ping ping = new Ping())
            {
                stopwatch.Start();
                var pingReply = await ping.SendPingAsync(_configuration.Target, TimeSpan.FromSeconds(_configuration.Timeout), cancellationToken: cancellationToken);
                stopwatch.Stop();

                return new MonitorResult
                {
                    IsSuccessful = pingReply.Status == IPStatus.Success,
                    Message = (pingReply.Status == IPStatus.Success) ? "" : $"Failed to send ping: {pingReply.Status.ToString()}",
                    ResponseTime = pingReply.RoundtripTime > 0 || !Stopwatch.IsHighResolution ? TimeSpan.FromMilliseconds(pingReply.RoundtripTime) : stopwatch.Elapsed
                };
            }
        }
    }
}
