using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.Common;
using UptimeMonitor.Core.Contracts.Api.StatusPages;
using UptimeMonitor.Core.Domain.StatusPages;
using UptimeMonitor.Web.Api.Data;
using UptimeMonitor.Web.Data;

namespace UptimeMonitor.Web.Controllers.Api.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/status-pages")]
    [EndpointGroupName("Admin")]
    public class StatusPageController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly MonitorDbContext _dbContext;

        public StatusPageController(IMapper mapper, MonitorDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet]
        [EndpointName("StatusPagesGetList")]
        public async Task<ActionResult<PagedListResponse<StatusPageSimpleDto>>> GetListAsync(int page = 0, int pageSize = 0, string? search = null)
        {
            var query = _dbContext.StatusPages.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Name.Contains(search));

            var configs = await query
                .OrderBy(m => m.Name)
                .Paginate(pageSize, page).ToListAsync();

            var totalRecords = await query.CountAsync();
            var totalPages = (pageSize == 0) ? 1 : (totalRecords + pageSize - 1) / pageSize;

            return Ok(new PagedListResponse<StatusPageSimpleDto>
            {
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                Records = configs.Select(_mapper.Map<StatusPageSimpleDto>)
            });
        }

        [HttpGet]
        [Route("{slug}")]
        [EndpointName("StatusPagesGetBySlug")]
        public async Task<ActionResult<StatusPageConfigResponse>> GetBySlugAsync(string slug)
        {
            var statusPage = await _dbContext.StatusPages
                .Include(x => x.MonitorGroups.OrderBy(x => x.SortOrder))
                    .ThenInclude(x => x.Monitors.OrderBy(x => x.SortOrder))
                        .ThenInclude(x => x.Monitor)
                .FirstOrDefaultAsync(x => x.Slug == slug);

            var x = _mapper.Map<StatusPageConfigResponse>(statusPage);

            return Ok(_mapper.Map<StatusPageConfigResponse>(statusPage));
        }

        [HttpPost]
        [EndpointName("StatusPagesCreate")]
        public async Task<ActionResult<StatusPageConfigResponse>> CreateAsync(CreateStatusPageRequest data)
        {
            var monitorIds = data.MonitorGroups.SelectMany(g => g.MonitorIds)
                .ToList();

            var monitors = await _dbContext.MonitorConfigurations.Where(m => monitorIds.Contains(m.Id))
                .ToListAsync();

            var monitorGroups = data.MonitorGroups.Select(g =>
            {
                var groupMonitors = monitors.Where(m => g.MonitorIds.Contains(m.Id))
                    .ToList();

                var group = new StatusPageMonitorGroup
                {
                    Name = g.Name
                };

                var order = 0;
                foreach (var monitor in groupMonitors)
                {
                    group.Monitors.Add(new StatusPageMonitorGroupMonitor
                    {
                        SortOrder = order,
                        Monitor = monitor
                    });

                    order++;
                }

                return group;
            }).ToList();

            var statusPage = new StatusPage
            {
                Slug = data.Slug,
                Name = data.Name
            };

            statusPage.MonitorGroups.AddRange(monitorGroups);

            await _dbContext.StatusPages.AddAsync(statusPage);
            await _dbContext.SaveChangesAsync();

            return Ok(statusPage);
        }

        [HttpPost]
        [Route("{slug}/set-default")]
        [EndpointName("StatusPagesSetDefault")]
        public async Task<ActionResult> SetDefaultAsync(string slug)
        {
            var statusPages = await _dbContext.StatusPages
                .Where(x => x.Slug == slug || x.Default == true)
                .ToListAsync();

            if (!statusPages.Any(x => x.Slug == slug))
            {
                return NotFound();
            }

            foreach (var page in statusPages)
            {
                page.Default = page.Slug == slug;
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("{slug}")]
        [EndpointName("StatusPagesUpdate")]
        public async Task<ActionResult<StatusPageConfigResponse>> UpdateAsync(string slug, UpdateStatusPageRequest data)
        {
            var statusPage = await _dbContext.StatusPages
                .Include(s => s.MonitorGroups.OrderBy(x => x.SortOrder))
                    .ThenInclude(mg => mg.Monitors.OrderBy(x => x.SortOrder))
                        .ThenInclude(m => m.Monitor)
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (statusPage == null)
            {
                return NotFound();
            }

            if (data.Slug != statusPage.Slug)
            {
                var slugExists = await _dbContext.StatusPages.AnyAsync(s => s.Slug == data.Slug);
                if (slugExists)
                {
                    return Conflict("A status page with the given slug already exists.");
                }
                statusPage.Slug = data.Slug;
            }

            if (data.Name != statusPage.Name)
            {
                statusPage.Name = data.Name;
            }

            var groupIds = data.MonitorGroups.Where(mg => mg.Id != null)
                .Select(mg => mg.Id!.Value).ToHashSet();
            statusPage.MonitorGroups.RemoveAll(mg => !groupIds.Contains(mg.Id));

            var allMonitorIds = data.MonitorGroups
                .SelectMany(g => g.MonitorIds)
                .Distinct()
                .ToList();

            var monitorDict = await _dbContext.MonitorConfigurations
                .Where(m => allMonitorIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            var groupOrder = 0;
            foreach (var group in data.MonitorGroups)
            {
                if (group.Id != null)
                {
                    // Check if we can find the group by id
                    var existingGroup = statusPage.MonitorGroups.FirstOrDefault(g => g.Id == group.Id);

                    // If it exists, then update it
                    if (existingGroup != null)
                    {
                        if (existingGroup.Name != group.Name)
                        {
                            existingGroup.Name = group.Name;
                        }

                        if (existingGroup.SortOrder != groupOrder)
                        {
                            existingGroup.SortOrder = groupOrder;
                        }

                        var monitorIds = group.MonitorIds.ToHashSet();
                        existingGroup.Monitors.RemoveAll(m => !monitorIds.Contains(m.Monitor.Id));

                        var monitorOrder = 0;
                        foreach (var monitorId in monitorIds)
                        {
                            if (!existingGroup.Monitors.Any(m => m.Monitor.Id == monitorId))
                            {
                                if (monitorDict.TryGetValue(monitorId, out var monitor))
                                {
                                    existingGroup.Monitors.Add(new StatusPageMonitorGroupMonitor
                                    {
                                        SortOrder = monitorOrder,
                                        Monitor = monitor
                                    });
                                }
                            }
                            else
                            {
                                var existingMonitor = existingGroup.Monitors.First(m => m.Monitor.Id == monitorId);
                                if (existingMonitor.SortOrder != monitorOrder)
                                {
                                    existingMonitor.SortOrder = monitorOrder;
                                }
                            }

                            monitorOrder++;
                        }
                    }
                }
                else
                {
                    // Add new group to the status page
                    var newGroup = new Core.Domain.StatusPages.StatusPageMonitorGroup
                    {
                        Name = group.Name,
                        SortOrder = groupOrder
                    };

                    var order = 0;
                    foreach (var monitorId in group.MonitorIds)
                    {
                        if (monitorDict.TryGetValue(monitorId, out var monitor))
                        {
                            newGroup.Monitors.Add(new StatusPageMonitorGroupMonitor
                            {
                                SortOrder = order,
                                Monitor = monitor
                            });
                            order++;
                        }
                    }

                    statusPage.MonitorGroups.Add(newGroup);
                }

                groupOrder++;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(_mapper.Map<StatusPageConfigResponse>(statusPage));
        }

        [HttpDelete]
        [Route("{slug}")]
        [EndpointName("StatusPagesDelete")]
        public async Task<ActionResult> DeleteAsync(string slug)
        {
            var statusPage = await _dbContext.StatusPages.FirstOrDefaultAsync(x => x.Slug == slug);

            if (statusPage == null)
            {
                return NotFound();
            }

            _dbContext.StatusPages.Remove(statusPage);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
