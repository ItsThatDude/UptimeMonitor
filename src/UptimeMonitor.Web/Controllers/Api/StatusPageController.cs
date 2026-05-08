using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.StatusPages;
using UptimeMonitor.Web.Api.Data;
using UptimeMonitor.Web.Api.Services;

namespace UptimeMonitor.Web.Controllers.Api
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/status-page")]
    [EndpointGroupName("Public")]
    public class StatusPageController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly MonitorDbContext _dbContext;
        private readonly HeartbeatService _heartbeatService;

        public StatusPageController(IMapper mapper, MonitorDbContext dbContext, HeartbeatService heartbeatService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _heartbeatService = heartbeatService;
        }

        [HttpGet]
        [Route("{slug}")]
        [EndpointName("GetStatusPage")]
        public async Task<ActionResult<GetStatusPageResponse>> GetAsync(string slug)
        {
            var statusPage = await _dbContext.StatusPages
                .Include(x => x.MonitorGroups.OrderBy(x => x.SortOrder))
                    .ThenInclude(x => x.Monitors.OrderBy(x => x.SortOrder))
                            .ThenInclude(x => x.Monitor)
                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (statusPage == null)
                return NotFound();

            return Ok(_mapper.Map<GetStatusPageResponse>(statusPage));
        }

        [HttpGet]
        [Route("{slug}/heartbeats")]
        [EndpointName("GetHeartbeats")]
        public async Task<ActionResult<IEnumerable<GetMonitorHeartbeatsResponse>>> GetHeartbeatsAsync(
            string slug,
            [FromQuery] string? location = null,
            [FromQuery] string? worker = null,
            [FromQuery] string? timeRange = null)
        {
            var interval = TimeSpan.FromMinutes(1);
            var limit = 61;

            if (timeRange != null && timeRange == "24h")
            {
                interval = TimeSpan.FromMinutes(30);
                limit = 49;
            }

            var monitorIds = await _dbContext.StatusPages
                .Where(sp => sp.Slug == slug)
                .SelectMany(sp => sp.MonitorGroups.SelectMany(mg => mg.Monitors.Select(m => m.Monitor.Id)))
                .ToListAsync();

            if (!monitorIds.Any())
                return Ok(Enumerable.Empty<GetMonitorHeartbeatsResponse>());

            var heartbeats = await _heartbeatService.GetHeartbeatsAsync(monitorIds, interval, limit, location, worker);

            return Ok(heartbeats);
        }

        [HttpGet]
        [Route("{slug}/locations")]
        [EndpointName("GetLocations")]
        public async Task<ActionResult<IEnumerable<string>>> GetLocationsAsync(string slug, [FromQuery] string? search)
        {
            var statusPage = await _dbContext.StatusPages.FirstOrDefaultAsync(page => page.Slug == slug);

            if (statusPage == null)
            {
                return NotFound();
            }

            var query = _dbContext.WorkerConfigurations.Select(w => w.Location);

            if (search != null)
            {
                query = query.Where(location => location.ToLower().Contains(search.ToLower()));
            }

            return await query.Distinct()
                .ToListAsync();
        }

        [HttpGet]
        [Route("{slug}/workers")]
        [EndpointName("GetWorkers")]
        public async Task<ActionResult<IEnumerable<string>>> GetWorkersAsync(string slug, [FromQuery] string? search, [FromQuery] string? location)
        {
            var statusPage = await _dbContext.StatusPages.FirstOrDefaultAsync(page => page.Slug == slug);

            if (statusPage == null)
            {
                return NotFound();
            }

            var query = _dbContext.WorkerConfigurations.AsQueryable();

            if (location != null)
            {
                query = query.Where(worker => worker.Location.ToLower() == location.ToLower());
            }

            if (search != null)
            {
                query = query.Where(worker => worker.Name.ToLower().Contains(search.ToLower()));
            }

            return await query.Select(worker => worker.Name).Distinct()
                .ToListAsync();
        }
    }
}
