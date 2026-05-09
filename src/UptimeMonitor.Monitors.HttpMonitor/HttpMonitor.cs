using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using UptimeMonitor.Core.Plugins.Monitoring;
using UptimeMonitor.Core.Plugins.PluginBase.Cryptography;
using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Monitors.HttpMonitor
{
    public class HttpMonitor : IMonitorType, IDisposable
    {
        private ILogger<HttpMonitor>? _logger;

        public string Name => nameof(HttpMonitor);
        public string DisplayName => "Http Monitor";

        private MonitorConfiguration? _configuration;

        private HttpMonitorSettings? _settings;

        private X509Certificate2? _certificate;

        private HttpMessageHandler? _httpMessageHandler;
        private HttpClient? _httpClient;

        public void UpdateConfiguration(MonitorConfiguration configuration)
        {
            _configuration = configuration;
            _settings = JsonSerializer.Deserialize<HttpMonitorSettings>(_configuration.Settings);

            if (_settings == null)
            {
                throw new Exception("Unable to parse settings JSON");
            }

            if (_httpMessageHandler == null)
            {
                var sslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = CertificateValidationCallback
                };

                var handler = new SocketsHttpHandler
                {
                    AllowAutoRedirect = false,
                    SslOptions = sslOptions,
                    //PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    UseProxy = _settings.UseProxy
                };

                if (!string.IsNullOrWhiteSpace(_settings.ProxyServer))
                {
                    handler.Proxy = new HttpMonitorProxy(new Uri(_settings.ProxyServer));
                }

                _httpMessageHandler = handler;
            }

            if (_httpClient == null)
            {
                _httpClient = new HttpClient(_httpMessageHandler, false);
            }
        }

        public void ConfigureServices(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<HttpMonitor>>();
        }

        private bool CertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (_configuration == null)
            {
                throw new Exception("Monitor has not been configured");
            }

            if (_settings == null)
            {
                throw new Exception("HttpMonitorSettings was null");
            }

            if (_logger == null)
            {
                throw new Exception("You must call ConfigureServices before running CheckAsync");
            }

            _logger.LogDebug($"[{_configuration.Target}] Target: {_configuration.Target}");
            _logger.LogDebug($"[{_configuration.Target}] Errors: {sslPolicyErrors}");

            if (certificate == null)
            {
                _logger.LogWarning($"[{_configuration.Target}] No certificate provided");
                return false;
            }
            else
            {
                _logger.LogDebug($"[{_configuration.Target}] Effective date: {certificate?.GetEffectiveDateString()}");
                _logger.LogDebug($"[{_configuration.Target}] Exp date: {certificate?.GetExpirationDateString()}");
                _logger.LogDebug($"[{_configuration.Target}] Issuer: {certificate?.Issuer}");
                _logger.LogDebug($"[{_configuration.Target}] Subject: {certificate?.Subject}");

                _certificate = new X509Certificate2(certificate!);

                if (_settings.IgnoreInvalidCertificates || sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(_settings.CACertificate) && certificate != null)
                {
                    try
                    {
                        var ca = X509Certificate2.CreateFromPem(_settings.CACertificate);
                        var isSignedBy = _certificate.IsSignedBy(ca);

                        _logger.LogDebug($"[{_configuration.Target}] Loaded signer certificate with subject: {ca.Subject}");
                        _logger.LogDebug($"[{_configuration.Target}] Exp date: {ca.GetExpirationDateString()}");
                        _logger.LogDebug($"[{_configuration.Target}] Is signed by chain: {isSignedBy}");

                        if (isSignedBy)
                        {
                            return true;
                        }
                    }
                    catch (CryptographicException ex)
                    {
                        _logger.LogError(ex, "Failed to load CA Certificate: {error}", ex.Message);
                    }
                }

                _logger.LogWarning($"Certificate {certificate?.Subject} is not trusted");

                return false;
            }
        }

        public async Task<MonitorResult> CheckAsync(CancellationToken cancellationToken)
        {
            if (_configuration == null)
            {
                throw new Exception("Monitor has not been configured");
            }

            if (_settings == null)
            {
                throw new Exception("HttpMonitorSettings was null");
            }

            if (_logger == null)
            {
                throw new Exception("You must call ConfigureServices before running CheckAsync");
            }

            if (_httpClient == null)
            {
                throw new Exception("HttpClient is not initialized");
            }

            if (_configuration.Timeout == 0)
            {
                throw new Exception("Timeout must be greater than 0");
            }

            var stopwatch = new Stopwatch();

            try
            {
                var request = new HttpRequestMessage(new HttpMethod(_settings.Method), _configuration.Target);

                using (var listener = new HttpEventListener())
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_configuration.Timeout));
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

                    stopwatch.Start();
                    var response = await _httpClient.SendAsync(request, linkedCts.Token);
                    stopwatch.Stop();

                    bool success = _settings.AcceptedStatusCodes.Contains((int)response.StatusCode);

                    MonitorTlsDetails? tlsDetails = null;

                    if (_certificate != null)
                    {
                        tlsDetails = new MonitorTlsDetails
                        {
                            Subject = _certificate.Subject,
                            Issuer = _certificate.Issuer,
                            NotBefore = TimeZoneInfo.ConvertTimeToUtc(_certificate.NotBefore, TimeZoneInfo.Local),
                            NotAfter = TimeZoneInfo.ConvertTimeToUtc(_certificate.NotAfter, TimeZoneInfo.Local),
                        };
                    }

                    TimeSpan responseTime;
                    var timings = listener.GetTimings();

                    switch (_settings.ResponseTimeMeasurement)
                    {
                        default:
                        case ResponseTimeMeasurement.Request:
                            responseTime = timings.Request ?? stopwatch.Elapsed;
                            break;
                        case ResponseTimeMeasurement.SslHandshake:
                            responseTime = listener.GetTimings().SslHandshake ?? stopwatch.Elapsed;
                            break;
                        case ResponseTimeMeasurement.SocketConnect:
                            responseTime = listener.GetTimings().SocketConnect ?? stopwatch.Elapsed;
                            break;
                        case ResponseTimeMeasurement.RequestHeaders:
                            responseTime = listener.GetTimings().RequestHeaders ?? stopwatch.Elapsed;
                            break;
                        case ResponseTimeMeasurement.ResponseHeaders:
                            responseTime = listener.GetTimings().ResponseHeaders ?? stopwatch.Elapsed;
                            break;
                        case ResponseTimeMeasurement.ResponseContent:
                            responseTime = listener.GetTimings().ResponseContent ?? stopwatch.Elapsed;
                            break;
                        case ResponseTimeMeasurement.TimeToHeaders:
                            responseTime = listener.GetTimings().TimeToHeaders ?? stopwatch.Elapsed;
                            break;
                        case ResponseTimeMeasurement.Internal:
                            responseTime = stopwatch.Elapsed;
                            break;
                    }

                    return new MonitorResult
                    {
                        IsSuccessful = success,
                        Message = $"Status Code: {response.StatusCode}",
                        ResponseTime = responseTime,
                        TlsDetails = tlsDetails
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred sending the HTTP Request: {error}", ex.Message);

                stopwatch.Stop();
                return new MonitorResult
                {
                    IsSuccessful = false,
                    Message = $"Error: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed
                };
            }
        }

        public void Dispose()
        {
            if (_certificate != null)
            {
                _certificate.Dispose();
                _certificate = null;
            }

            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }

            if (_httpMessageHandler != null)
            {
                _httpMessageHandler.Dispose();
                _httpMessageHandler = null;
            }
        }
    }
}
