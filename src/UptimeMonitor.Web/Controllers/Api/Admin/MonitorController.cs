using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.Common;
using UptimeMonitor.Core.Contracts.Api.StatusPages;
using UptimeMonitor.Core.Domain.Monitors;
using UptimeMonitor.Core.Plugins;
using UptimeMonitor.Web.Api.Data;
using UptimeMonitor.Web.Api.Services;
using UptimeMonitor.Web.Data;

namespace UptimeMonitor.Web.Controllers.Api.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/monitors")]
    [EndpointGroupName("Admin")]
    public class MonitorController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly MonitorDbContext _dbContext;
        private readonly PluginManager _pluginManager;
        private readonly HeartbeatService _heartbeatService;

        public MonitorController(
            IMapper mapper,
            MonitorDbContext dbContext,
            PluginManager pluginManager,
            HeartbeatService heartbeatService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _pluginManager = pluginManager;
            _heartbeatService = heartbeatService;
        }

        [HttpGet("statistics")]
        [EndpointName("AdminGetMonitorStatistics")]
        public async Task<ActionResult<GetMonitorStatisticsResponse>> GetStatisticsAsync()
        {
            var totalMonitors = await _dbContext.MonitorConfigurations.CountAsync();
            var enabledMonitors = await _dbContext.MonitorConfigurations.Where(mc => mc.Enabled).CountAsync();
            var disabledMonitors = await _dbContext.MonitorConfigurations.Where(mc => !mc.Enabled).CountAsync();

            var monitors = _dbContext.MonitorResults.OrderByDescending(mr => mr.Timestamp)
                .GroupBy(mr => mr.MonitorConfigurationId);

            var upMonitors = monitors.Where((grouping) => grouping.OrderByDescending(mr => mr.Timestamp).Take(1).Any(mr => mr.IsSuccessful)).Count();
            var downMonitors = monitors.Where((grouping) => grouping.OrderByDescending(mr => mr.Timestamp).Take(1).Any(mr => !mr.IsSuccessful)).Count();

            return Ok(new GetMonitorStatisticsResponse
            {
                TotalMonitors = totalMonitors,
                EnabledMonitors = enabledMonitors,
                DisabledMonitors = disabledMonitors,
                UpMonitors = upMonitors,
                DownMonitors = downMonitors
            });
        }

        [HttpGet]
        [EndpointName("AdminGetMonitors")]
        public async Task<ActionResult<PagedListResponse<MonitorSimpleDto>>> GetListAsync(int page = 0, int pageSize = 0, string? search = null)
        {
            var query = _dbContext.MonitorConfigurations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Name.Contains(search));

            var configs = await query
                .OrderBy(m => m.Name).ThenBy(m => m.Type)
                .Paginate(pageSize, page).ToListAsync();

            var totalRecords = await query.CountAsync();
            var totalPages = (pageSize == 0) ? 1 : (totalRecords + pageSize - 1) / pageSize;

            return Ok(new PagedListResponse<MonitorSimpleDto>
            {
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                Records = configs.Select(_mapper.Map<MonitorSimpleDto>)
            });
        }

        [HttpGet("events")]
        [EndpointName("AdminGetAllMonitorEvents")]
        public async Task<ActionResult<PagedListResponse<MonitorEventDto>>> GetAllEventsAsync(int page = 0, int pageSize = 0)
        {
            var events = await _dbContext.MonitorEvents
                .Include(e => e.Monitor).OrderByDescending(e => e.Timestamp)
                .Paginate(pageSize, page).ToListAsync();

            var totalRecords = await _dbContext.MonitorEvents.CountAsync();
            var totalPages = (pageSize == 0) ? 1 : (totalRecords + pageSize - 1) / pageSize;

            return Ok(new PagedListResponse<MonitorEventDto>
            {
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                Records = events.Select(_mapper.Map<MonitorEventDto>)
            });
        }

        [HttpGet("types")]
        [EndpointName("AdminGetMonitorTypes")]
        public ActionResult<IEnumerable<MonitorTypeDto>> GetMonitorTypes()
        {
            // todo this might need to be rethought...
            return Ok(_pluginManager.GetMonitorTypeNames().Select((kvp) => new MonitorTypeDto { Key = kvp.Key, DisplayName = kvp.Value }));
        }

        [HttpGet("type-settings")]
        [EndpointName("AdminGetMonitorTypeSettings")]
        public ActionResult<IEnumerable<MonitorSettingsSchemaDto>> GetMonitorTypeSettings()
        {
            var settingsSchemas = _pluginManager.GetMonitorTypeSettingsSchemas();

            var dtos = settingsSchemas.Select((schema) =>
            {
                return new MonitorSettingsSchemaDto
                {
                    MonitorType = schema.Key,
                    Properties = schema.Value.Select((prop) =>
                    {
                        return new MonitorSettingsPropertyDto
                        {
                            Key = prop.Name,
                            DataType = prop.DataType.ToString(),
                            DisplayName = prop.DisplayName,
                            Required = prop.Required,
                            AllowMultiple = prop.AllowMultiple,
                            Description = prop.Description,
                            DefaultValue = prop.DefaultValue
                        };
                    })
                };
            });

            return Ok(dtos);
        }

        [HttpPost]
        [EndpointName("AdminCreateMonitor")]
        public async Task<ActionResult<MonitorConfigurationResponse>> CreateAsync(CreateMonitorConfigRequest config)
        {
            var newMonitor = new MonitorConfiguration
            {
                Name = config.Name,
                Type = config.Type,
                Target = config.Target,
                Enabled = config.Enabled,
                Interval = config.Interval,
                Timeout = config.Timeout,
                WarningThreshold = config.WarningThreshold,
                Settings = config.Settings
            };

            await _dbContext.MonitorConfigurations.AddAsync(newMonitor);
            await _dbContext.SaveChangesAsync();

            return Ok(_mapper.Map<MonitorConfigurationResponse>(newMonitor));
        }

        [HttpGet("{id}")]
        [EndpointName("AdminGetMonitor")]
        public async Task<ActionResult<MonitorConfigurationResponse>> GetAsync(int id)
        {
            var config = await _dbContext.MonitorConfigurations
                .Include(m => m.TlsDetails).FirstOrDefaultAsync(m => m.Id == id);

            if (config == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<MonitorConfigurationResponse>(config));
        }

        [HttpGet("{id}/events")]
        [EndpointName("AdminGetMonitorEvents")]
        public async Task<ActionResult<PagedListResponse<MonitorEventDto>>> GetEventsAsync(int id, int page = 0, int pageSize = 0)
        {
            var query = _dbContext.MonitorEvents.Where(e => e.MonitorId == id);
            var events = await query
                .Include(e => e.Monitor).OrderByDescending(e => e.Timestamp)
                .Paginate(pageSize, page).ToListAsync();

            var totalRecords = await query.CountAsync();
            var totalPages = (pageSize == 0) ? 1 : (totalRecords + pageSize - 1) / pageSize;

            return Ok(new PagedListResponse<MonitorEventDto>
            {
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                Records = events.Select(_mapper.Map<MonitorEventDto>)
            });
        }

        [HttpGet("{id}/metrics")]
        [EndpointName("AdminGetChartDataForMonitor")]
        public async Task<ActionResult<GetMonitorHeartbeatsResponse>> GetChartDataForMonitorAsync(int id)
        {
            var config = await _dbContext.MonitorConfigurations.FindAsync(id);

            if (config == null)
            {
                return NotFound();
            }

            var heartbeats = await _heartbeatService.GetHeartbeatsAsync(id, TimeSpan.FromMinutes(1), 60);

            return Ok(heartbeats);
        }

        [HttpGet("metrics")]
        [EndpointName("AdminGetChartDataForMonitors")]
        public async Task<ActionResult<IEnumerable<GetMonitorHeartbeatsResponse>>> GetChartDataAsync([FromQuery] int[] ids)
        {
            var heartbeats = await _heartbeatService.GetHeartbeatsAsync(ids, TimeSpan.FromMinutes(1), 60);

            return Ok(heartbeats);
        }

        [HttpGet("metrics/all")]
        [EndpointName("AdminGetChartDataAllMonitors")]
        public async Task<ActionResult<IEnumerable<GetMonitorHeartbeatsResponse>>> GetChartDataAsync()
        {
            var ids = await _dbContext.MonitorConfigurations.Select(mc => mc.Id).ToListAsync();

            var heartbeats = await _heartbeatService.GetHeartbeatsAsync(ids, TimeSpan.FromMinutes(1), 60);

            return Ok(heartbeats);
        }

        [HttpPut("{id}")]
        [EndpointName("AdminUpdateMonitor")]
        public async Task<ActionResult<MonitorConfigurationResponse>> UpdateAsync(int id, UpdateMonitorConfigRequest data)
        {
            var monitor = await _dbContext.MonitorConfigurations.FindAsync(id);

            if (monitor == null)
            {
                return NotFound();
            }

            if (monitor.Name != data.Name)
            {
                monitor.Name = data.Name;
            }

            if (monitor.Target != data.Target)
            {
                monitor.Target = data.Target;
            }

            if (monitor.Enabled != data.Enabled)
            {
                monitor.Enabled = data.Enabled;
            }

            if (monitor.Interval != data.Interval)
            {
                monitor.Interval = data.Interval;
            }

            if (monitor.Timeout != data.Timeout)
            {
                monitor.Timeout = data.Timeout;
            }

            if (monitor.WarningThreshold != data.WarningThreshold)
            {
                monitor.WarningThreshold = data.WarningThreshold;
            }

            if (monitor.Settings != data.Settings)
            {
                monitor.Settings = data.Settings;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(_mapper.Map<MonitorConfigurationResponse>(monitor));
        }

        [HttpDelete("{id}")]
        [EndpointName("AdminDeleteMonitor")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var monitor = await _dbContext.MonitorConfigurations.FindAsync(id);

            if (monitor == null)
            {
                return NotFound();
            }

            _dbContext.MonitorConfigurations.Remove(monitor);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
