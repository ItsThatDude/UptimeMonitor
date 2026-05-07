using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using UptimeMonitor.Core.Plugins.Monitoring;
using UptimeMonitor.Core.Plugins.PluginBase.Cryptography;
using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Monitors.TCPMonitor
{
    public class TcpMonitor : IMonitorType
    {
        private ILogger<TcpMonitor>? _logger;

        public string Name => nameof(TcpMonitor);
        public string DisplayName => "Tcp Monitor";

        private MonitorConfiguration? _configuration;

        public void UpdateConfiguration(MonitorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<TcpMonitor>>();
        }

        public async Task<MonitorResult> CheckAsync(CancellationToken cancellationToken)
        {
            if (_configuration == null)
            {
                throw new Exception("Monitor has not been configured");
            }

            if (_logger == null)
            {
                throw new Exception("You must call ConfigureServices before running CheckAsync");
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Deserialize the settings JSON for the timeout
                var settings = JsonSerializer.Deserialize<TcpMonitorSettings>(_configuration.Settings)
                               ?? throw new Exception("Invalid settings JSON");

                // Extract hostname and port from the target parameter
                var (hostname, port) = ParseTarget(_configuration.Target);

                using var client = new TcpClient();
                using (var listener = new SocketEventListener())
                {
                    // Use ConnectAsync with timeout/cancellation instead of the legacy BeginConnect/EndConnect pattern
                    var connectTask = client.ConnectAsync(hostname, port);

                    var timeout = TimeSpan.FromSeconds(_configuration.Timeout);
                    using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        var delayTask = Task.Delay(timeout, timeoutCts.Token);

                        var completed = await Task.WhenAny(connectTask, delayTask);

                        if (completed != connectTask)
                        {
                            throw new TimeoutException($"TCP connection to {hostname}:{port} timed out.");
                        }

                        // propagate exceptions from the connect task
                        await connectTask;

                        // cancel the delay if connect succeeded
                        timeoutCts.Cancel();
                    }

                    MonitorTlsDetails? tlsDetails = null;

                    if (settings.UseSSL)
                    {
                        using SslStream ssl = new SslStream(
                            client.GetStream(),
                            false,
                            new RemoteCertificateValidationCallback(
                                (sender, certificate, chain, errors) =>
                                {
                                    if (certificate == null)
                                    {
                                        return errors == SslPolicyErrors.None || settings.IgnoreInvalidCertificates;
                                    }

                                    var cert = new X509Certificate2(certificate);

                                    tlsDetails = new MonitorTlsDetails
                                    {
                                        Subject = cert.Subject,
                                        Issuer = cert.Issuer,
                                        NotBefore = TimeZoneInfo.ConvertTimeToUtc(cert.NotBefore, TimeZoneInfo.Local),
                                        NotAfter = TimeZoneInfo.ConvertTimeToUtc(cert.NotAfter, TimeZoneInfo.Local),
                                    };

                                    if (errors == SslPolicyErrors.None || settings.IgnoreInvalidCertificates)
                                        return true;

                                    if (!string.IsNullOrEmpty(settings.CACertificate) && certificate != null)
                                    {
                                        var ca = X509Certificate2.CreateFromPem(settings.CACertificate);
                                        var isSignedBy = cert.IsSignedBy(ca);

                                        if (isSignedBy)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }
                            ),
                            null
                        );

                        await ssl.AuthenticateAsClientAsync(hostname);

                        if (ssl.RemoteCertificate == null)
                        {
                            _logger.LogInformation("No certificate found in ssl stream");
                        }
                    }

                    // EndConnect no longer needed when using ConnectAsync

                    stopwatch.Stop();
                    return new MonitorResult
                    {
                        IsSuccessful = true,
                        Message = "TCP Connection Successful",
                        ResponseTime = listener.GetTimings().SocketConnect ?? stopwatch.Elapsed,
                        TlsDetails = tlsDetails
                    };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new MonitorResult
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed
                };
            }
        }

        // Parse the target string into hostname and port
        private (string hostname, int port) ParseTarget(string target)
        {
            var parts = target.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
            {
                throw new ArgumentException("Invalid target format. Expected 'hostname:port'.");
            }

            return (parts[0], port);
        }
    }
}
