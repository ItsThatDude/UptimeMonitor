using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.Workers;
using UptimeMonitor.Core.Domain.Monitors;
using UptimeMonitor.Core.Domain.Workers;
using UptimeMonitor.Core.Plugins;
using UptimeMonitor.Web.Api.Auth;
using UptimeMonitor.Web.Api.Data;
using UptimeMonitor.Web.Api.Services;

namespace UptimeMonitor.Web.Controllers.Api
{
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("api/worker")]
    [EndpointGroupName("Worker")]
    public class WorkerController : ControllerBase
    {
        private readonly ILogger<WorkerController> _logger;
        private readonly IMapper _mapper;
        private readonly MonitorDbContext _dbContext;
        private readonly WorkerService _workerService;
        private readonly WorkerTokenService _tokenService;
        private readonly PluginManager _pluginManager;

        public WorkerController(ILogger<WorkerController> logger,
            IMapper mapper,
            MonitorDbContext dbContext,
            WorkerService workerService,
            WorkerTokenService tokenService,
            PluginManager pluginManager)
        {
            _logger = logger;
            _mapper = mapper;
            _dbContext = dbContext;
            _workerService = workerService;
            _tokenService = tokenService;
            _pluginManager = pluginManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [EndpointName("Register")]
        public async Task<ActionResult<string>> RegisterWorker([FromBody] WorkerRegistrationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.WorkerId) || request.Location == null)
            {
                return BadRequest("Invalid worker data.");
            }

            var exists = await _workerService.ExistsAsync(request.WorkerId);

            if (exists)
            {
                throw new Exception("A worker with this ID already exists");
            }

            var secret = Cryptography.GenerateSecureSecret();

            // Store worker info
            await _workerService.RegisterWorker(request.WorkerId, request.Location, secret);

            return Ok(secret);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        [EndpointName("Authenticate")]
        public async Task<ActionResult<WorkerTokenResponse>> Authenticate([FromBody] WorkerAuthenticationRequest request)
        {
            var hashedSecret = Cryptography.HashSecret(request.Secret);

            var worker = await _dbContext.WorkerConfigurations.FirstOrDefaultAsync(w => w.Name == request.WorkerId);

            if (worker == null)
            {
                _logger.LogWarning("Worker not found: {WorkerId}", request.WorkerId);
                return Unauthorized();
            }

            var validHash = Cryptography.VerifySecret(request.Secret, worker.Secret);

            if (!validHash)
            {
                _logger.LogWarning("Invalid secret for worker: {WorkerId}", request.WorkerId);
                _logger.LogWarning("Secret: {Secret}; Hashed Secret: {hashedSecret}", request.Secret, hashedSecret);
                return Unauthorized();
            }

            var newToken = _tokenService.GenerateToken(worker.Name, worker.Location);

            return Ok(new WorkerTokenResponse { Token = newToken.Token, Expires = newToken.Expires });
        }

        [Authorize(Policy = "WorkerOnly")]
        [HttpGet("plugins")]
        [EndpointName("GetPlugins")]
        public ActionResult<IEnumerable<MonitorPluginDto>> GetPlugins()
        {
            return Ok(_pluginManager.Plugins.Select(_mapper.Map<MonitorPluginDto>));
        }

        [Authorize(Policy = "WorkerOnly")]
        [HttpGet("config")]
        [EndpointName("GetConfig")]
        public async Task<ActionResult<Core.Contracts.Api.Workers.WorkerConfiguration>> GetConfigAsync()
        {
            var worker = await GetWorkerAsync();

            if (worker == null)
            {
                return Unauthorized();
            }

            var configuration = await _dbContext.WorkerConfigurations.FirstOrDefaultAsync(w => w.Name == worker.Name);

            if (configuration == null)
            {
                _logger.LogWarning("Worker configuration not found for ID: {WorkerId}", worker.Name);
                return NotFound();
            }
            else
            {
                configuration.LastCheckIn = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return base.Ok(_mapper.Map<Core.Contracts.Api.Workers.WorkerConfiguration>(configuration));
        }

        [Authorize(Policy = "WorkerOnly")]
        [HttpGet("monitors")]
        [EndpointName("GetMonitors")]
        public async Task<ActionResult<IEnumerable<MonitorConfigurationResponse>>> GetMonitorsAsync()
        {
            var worker = await GetWorkerAsync();

            if (worker == null)
            {
                return Unauthorized();
            }

            var monitors = await _dbContext.MonitorConfigurations
                .Where(mc => mc.Enabled == true).ToListAsync();

            return Ok(monitors.Select(_mapper.Map<MonitorConfigurationResponse>));
        }

        [Authorize(Policy = "WorkerOnly")]
        [HttpPost("report")]
        [EndpointName("ReportResult")]
        public async Task<IActionResult> ReportResultAsync(UploadMonitorResultResponse data)
        {
            var worker = await GetWorkerAsync();

            if (worker == null)
            {
                return Unauthorized();
            }

            var monitor = await GetMonitorAsync(data.MonitorId);

            if(monitor == null)
            {
                return NotFound();
            }

            if(data.TlsDetails != null)
            {
                if (monitor.TlsDetails == null)
                {
                    monitor.TlsDetails = new MonitorTlsDetails
                    {
                        Subject = data.TlsDetails.Subject,
                        Issuer = data.TlsDetails.Issuer,
                        NotBefore = data.TlsDetails.NotBefore,
                        NotAfter = data.TlsDetails.NotAfter
                    };
                }
                else
                {
                    if (monitor.TlsDetails.Subject != data.TlsDetails.Subject)
                    {
                        monitor.TlsDetails.Subject = data.TlsDetails.Subject;
                    }

                    if (monitor.TlsDetails.Issuer != data.TlsDetails.Issuer)
                    {
                        monitor.TlsDetails.Issuer = data.TlsDetails.Issuer;
                    }

                    if (monitor.TlsDetails.NotBefore != data.TlsDetails.NotBefore)
                    {
                        monitor.TlsDetails.NotBefore = data.TlsDetails.NotBefore;
                    }

                    if (monitor.TlsDetails.NotAfter != data.TlsDetails.NotAfter)
                    {
                        monitor.TlsDetails.NotAfter = data.TlsDetails.NotAfter;
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

            var lastResult = await _dbContext.MonitorResults
                    .Where(mr => mr.MonitorConfigurationId == data.MonitorId)
                    .OrderByDescending(mr => mr.Timestamp)
                    .FirstOrDefaultAsync();

            if(lastResult != null)
            {
                if(lastResult.IsSuccessful && !data.IsSuccessful)
                {
                    await _dbContext.MonitorEvents.AddAsync(new MonitorEvent
                    {
                        MonitorId = data.MonitorId,
                        Timestamp = DateTime.UtcNow,
                        EventType = "MonitorOffline",
                        EventMessage = "Monitored target has gone offline"
                    });
                }
                else if(!lastResult.IsSuccessful && data.IsSuccessful)
                {
                    await _dbContext.MonitorEvents.AddAsync(new MonitorEvent
                    {
                        MonitorId = data.MonitorId,
                        Timestamp = DateTime.UtcNow,
                        EventType = "MonitorOnline",
                        EventMessage = "Monitored target is back online"
                    });
                }

                if(monitor.WarningThreshold > 0 && lastResult.IsSuccessful && data.IsSuccessful)
                {
                    if(lastResult.ResponseTime.TotalMilliseconds <= monitor.WarningThreshold
                        && data.ResponseTime.TotalMilliseconds > monitor.WarningThreshold)
                    {
                        await _dbContext.MonitorEvents.AddAsync(new MonitorEvent
                        {
                            MonitorId = data.MonitorId,
                            Timestamp = DateTime.UtcNow,
                            EventType = "MonitorLatencyThreshold",
                            EventMessage = $"Target's latency ({data.ResponseTime.TotalMilliseconds}ms) is higher than warning threshold of {monitor.WarningThreshold}ms"
                        });
                    }
                }
            }

            var dbItem = new MonitorResultEntry()
            {
                WorkerConfigurationId = worker.Id,
                MonitorConfigurationId = data.MonitorId,
                Timestamp = data.Timestamp,
                IsSuccessful = data.IsSuccessful,
                Message = data.Message,
                ResponseTime = data.ResponseTime
            };

            await _dbContext.MonitorResults.AddAsync(dbItem);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "WorkerOnly")]
        [HttpPost("refresh-token")]
        [EndpointName("RefreshToken")]
        public async Task<ActionResult<WorkerTokenResponse>> RefreshToken()
        {
            var worker = await GetWorkerAsync();

            if (worker == null)
            {
                return Unauthorized();
            }

            var newToken = _tokenService.GenerateToken(worker.Name, worker.Location);

            return Ok(new WorkerTokenResponse { Token = newToken.Token, Expires = newToken.Expires });
        }

        private async Task<MonitorConfiguration?> GetMonitorAsync(int id)
        {
            var monitor = await _dbContext.MonitorConfigurations.Include(m => m.TlsDetails)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (monitor == null)
            {
                _logger.LogWarning("Monitor not found: {MonitorId}", id);
                return null;
            }

            return monitor;
        }

        private async Task<Core.Domain.Workers.WorkerConfiguration?> GetWorkerAsync()
        {
            var workerId = User.FindFirst("workerId")?.Value;

            if (string.IsNullOrEmpty(workerId))
            {
                _logger.LogWarning("Worker ID not found in claims.");
                return null;
            }

            var worker = await _dbContext.WorkerConfigurations.FirstOrDefaultAsync(w => w.Name == workerId);

            if (worker == null)
            {
                _logger.LogWarning("Worker not found: {WorkerId}", workerId);
                return null;
            }

            return worker;
        }
    }
}
