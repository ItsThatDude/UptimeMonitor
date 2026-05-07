using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.Workers;
using UptimeMonitor.Core.Plugins.Monitoring;
using UptimeMonitor.Service.MonitorService.Settings;

namespace UptimeMonitor.Service.MonitorService.Monitor
{
    public class WorkerApiService : IWorkerService
    {
        private readonly ILogger<WorkerApiService> _logger;

        private readonly IHttpClientFactory _httpClientFactory;
        private string? _jwtToken;
        private DateTime _tokenExpiration;
        private readonly AuthOptions _authOptions;
        private readonly SemaphoreSlim _authLock = new SemaphoreSlim(1, 1);

        // retry/backoff settings
        private const int MaxRetries = 3;
        private const int BaseBackoffMs = 200;
        private readonly Random _rng = new Random();

        // ToDo: make a more reliable method for checking registration, ie API Call to check registration state
        public bool IsRegistered
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_authOptions.Secret);
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _jwtToken != null && _tokenExpiration > DateTime.UtcNow;
            }
        }

        public WorkerApiService(IHttpClientFactory httpClientFactory, IOptions<AuthOptions> authOptions, ILogger<WorkerApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _authOptions = authOptions.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_authOptions.ApiBaseUrl))
            {
                throw new Exception("The Api Base URL is invalid");
            }
        }

        public async Task WaitForRegistrationAsync(CancellationToken cancellationToken)
        {
            while (!IsRegistered)
            {
                await RegisterAsync(cancellationToken);

                if (!IsRegistered)
                {
                    _logger.LogDebug("Not registered... waiting 1 minute to try again..");
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }

            while (!IsAuthenticated)
            {
                await EnsureAuthenticatedAsync(cancellationToken);

                if (!IsAuthenticated)
                {
                    _logger.LogDebug("Not authenticated, waiting 1 minute to try again..");
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }
        }

        public async Task<WorkerConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var request = () => new HttpRequestMessage(HttpMethod.Get, $"{_authOptions.ApiBaseUrl}/worker/config");
            var response = await SendAndReadJsonAsync<WorkerConfiguration>(request, true, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                if (response.Data != null)
                {
                    return response.Data;
                }
            }

            _logger.LogError($"Failed to retrieve worker configuration. Status: {response.StatusCode}");
            return null;
        }

        public async Task<List<MonitorConfigurationResponse>> GetMonitorConfigsAsync(CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var request = () => new HttpRequestMessage(HttpMethod.Get, $"{_authOptions.ApiBaseUrl}/worker/monitors");
            var response = await SendAndReadJsonAsync<List<MonitorConfigurationResponse>>(request, true, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                if (response.Data != null)
                {
                    return response.Data;
                }
            }

            _logger.LogError($"Failed to retrieve monitor configurations. Status: {response.StatusCode}");
            return new List<MonitorConfigurationResponse>();
        }

        public async Task<bool> ReportResultAsync(int monitorId, bool success, string message, DateTime timestamp, TimeSpan responseTime, MonitorTlsDetails? tlsDetails = null, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var payload = new UploadMonitorResultResponse
            {
                MonitorId = monitorId,
                Timestamp = timestamp,
                IsSuccessful = success,
                Message = message,
                ResponseTime = responseTime,
                TlsDetails = tlsDetails == null ? null : new MonitorTlsDetailsDto
                {
                    Subject = tlsDetails.Subject,
                    Issuer = tlsDetails.Issuer,
                    NotAfter = tlsDetails.NotAfter,
                    NotBefore = tlsDetails.NotBefore
                }
            };

            var request = () => new HttpRequestMessage(HttpMethod.Post, $"{_authOptions.ApiBaseUrl}/worker/report")
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await SendAsync(request, true, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully reported monitor result for monitor id {monitorId}.", monitorId);
                return true;
            }

            _logger.LogWarning("Failed to report monitor result for monitor id {monitorId}. Status: {StatusCode}", monitorId, response.StatusCode);
            return false;
        }

        private async Task RegisterAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_authOptions.Secret))
            {
                var payload = new WorkerRegistrationRequest
                {
                    WorkerId = _authOptions.WorkerId,
                    Location = _authOptions.Location
                };

                var request = () => new HttpRequestMessage(HttpMethod.Post, $"{_authOptions.ApiBaseUrl}/worker/register")
                {
                    Content = JsonContent.Create(payload)
                };

                var response = await SendAndReadJsonAsync<string>(request, true, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully registered worker with the UptimeMonitor Api");

                    var secret = response.Data;

                    if (string.IsNullOrWhiteSpace(secret))
                    {
                        _logger.LogError("The secret received from the UptimeMonitor Api was invalid.");
                        return;
                    }

                    SettingsHelpers.AddOrUpdateAppSetting("Auth:Secret", secret);

                    _authOptions.Secret = secret;
                }
                else
                {
                    _logger.LogError("Failed to register worker.");
                    return;
                }
            }
        }

        private async Task<bool> EnsureAuthenticatedAsync(CancellationToken cancellationToken)
        {
            // If we're already authenticated and token is valid for more than 60 seconds, return
            if (_jwtToken != null && DateTime.UtcNow < _tokenExpiration)
            {
                var timeRemaining = _tokenExpiration - DateTime.UtcNow;
                if (timeRemaining.TotalSeconds > 60) return true;
            }

            // Use a semaphore to prevent multiple simultaneous authentication attempts
            await _authLock.WaitAsync(cancellationToken);

            try
            {
                // If the token has expired or is null, authenticate to get a new token
                if (_jwtToken == null || DateTime.UtcNow >= _tokenExpiration)
                {
                    _logger.LogInformation("Fetching new JWT token...");
                    return await AuthenticateAsync(cancellationToken);
                }
                // If the token is valid but about to expire in 60 seconds, refresh it
                else
                {
                    var timeRemaining = _tokenExpiration - DateTime.UtcNow;
                    if (timeRemaining.TotalSeconds <= 60)
                    {
                        return await RefreshTokenAsync(cancellationToken);
                    }
                }
            }
            finally
            {
                // Release the semaphore
                _authLock.Release();
            }

            return false;
        }

        private async Task<bool> AuthenticateAsync(CancellationToken cancellationToken)
        {
            var request = () => new HttpRequestMessage(HttpMethod.Post, $"{_authOptions.ApiBaseUrl}/worker/authenticate")
            {
                Content = JsonContent.Create(new WorkerAuthenticationRequest
                {
                    WorkerId = _authOptions.WorkerId,
                    Secret = _authOptions.Secret
                })
            };

            var response = await SendAndReadJsonAsync<WorkerTokenResponse>(request, false, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Authentication failed: {response.StatusCode}");
                return false;
            }
            else
            {
                HandleToken(response.Data);
                return true;
            }
        }

        private async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken)
        {
            var request = () => new HttpRequestMessage(HttpMethod.Post, $"{_authOptions.ApiBaseUrl}/worker/refresh-token");
            var response = await SendAndReadJsonAsync<WorkerTokenResponse>(request, true, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Token Refresh failed: {response.StatusCode}");
                return false;
            }

            HandleToken(response.Data);
            return true;
        }

        private void HandleToken(WorkerTokenResponse? authResponse)
        {
            if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
            {
                throw new Exception("Invalid authentication response.");
            }

            _jwtToken = authResponse.Token;
            _tokenExpiration = authResponse.Expires;
            _logger.LogInformation("JWT token acquired, expires at {Expiration}", _tokenExpiration);
        }

        private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> requestFactory, bool authenticate, CancellationToken cancellationToken)
        {
            // Retry on HttpRequestException OR when we get 5xx OR 401 responses.
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500 || (authenticate && r.StatusCode == HttpStatusCode.Unauthorized))
                .WaitAndRetryAsync(
                    retryCount: MaxRetries,
                    sleepDurationProvider: retryAttempt =>
                    {
                        int jitter;
                        lock (_rng) { jitter = _rng.Next(0, 100); }
                        return TimeSpan.FromMilliseconds(BaseBackoffMs * (1 << (retryAttempt - 1)) + jitter);
                    },
                    onRetryAsync: async (outcome, timespan, retryNumber, context) =>
                    {
                        outcome.Result?.Dispose();

                        // If we received a 401, try to re-authenticate once before retrying
                        if (authenticate && outcome.Result != null && outcome.Result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            if (context.ContainsKey("auth-attempted"))
                            {
                                _logger.LogWarning("Previous re-authentication attempt failed, not retrying again.");
                                return;
                            }

                            _logger.LogInformation("Received Unauthorized response, clearing token and attempting re-authentication (retry {Retry})", retryNumber);
                            _jwtToken = null;

                            await _authLock.WaitAsync(cancellationToken);
                            try
                            {
                                // AuthenticateAsync will log failures; if it fails the next attempt will still occur
                                bool success = await AuthenticateAsync(cancellationToken);
                                context["auth-attempted"] = true;
                                
                                if(!success)
                                {
                                    _logger.LogWarning("Re-authentication failed during retry {Retry}", retryNumber);
                                }
                            }
                            finally
                            {
                                _authLock.Release();
                            }
                        }
                        else
                        {
                            _logger.LogDebug("Transient failure, retry {Retry} after {Delay}ms", retryNumber, (int)timespan.TotalMilliseconds);
                        }
                    });

            // Execute the HTTP call inside the policy so requestFactory runs per attempt
            return await retryPolicy.ExecuteAsync(async (ctx, ct) =>
            {
                using var request = requestFactory();

                if (authenticate && !string.IsNullOrEmpty(_jwtToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                }

                using var httpClient = _httpClientFactory.CreateClient();
                return await httpClient.SendAsync(request, ct);
            }, new Context(), cancellationToken);
        }

        private async Task<ApiResponse<T>> SendAndReadJsonAsync<T>(Func<HttpRequestMessage> requestFactory, bool authenticate, CancellationToken cancellationToken)
        {
            using var response = await SendAsync(requestFactory, authenticate, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<T>
                {
                    IsSuccessStatusCode = false,
                    StatusCode = response.StatusCode,
                    Data = default
                };
            }

            var data = await response.Content.ReadFromJsonAsync<T>(cancellationToken);

            return new ApiResponse<T>
            {
                IsSuccessStatusCode = true,
                StatusCode = response.StatusCode,
                Data = data
            };
        }
    }
}
