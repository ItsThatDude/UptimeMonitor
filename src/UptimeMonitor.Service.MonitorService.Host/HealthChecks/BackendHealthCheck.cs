using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using UptimeMonitor.Service.MonitorService.Settings;

namespace UptimeMonitor.Service.MonitorService.HealthChecks
{
    public class BackendHealthCheck : IHealthCheck
    {
        private readonly string _apiBaseUrl;
        private readonly IHttpClientFactory _httpClientFactory;

        public BackendHealthCheck(IHttpClientFactory httpClientFactory, IOptions<AuthOptions> authOptions)
        {
            _apiBaseUrl = authOptions.Value.ApiBaseUrl;
            _httpClientFactory = httpClientFactory;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_apiBaseUrl);

                // Perform a simple GET request to check if the backend is reachable
                var response = httpClient.GetAsync("/healthz", cancellationToken).GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        string.Format(
                            "Backend {0} is not reachable (response status code: {1})",
                            response.RequestMessage?.RequestUri,
                            response.StatusCode
                        )
                    ));
                }

                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Exception during backend health check", ex));
            }
        }
    }
}