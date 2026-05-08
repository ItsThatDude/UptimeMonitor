using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.Common;
using UptimeMonitor.Core.Contracts.Api.Workers;
using UptimeMonitor.Web.Api.Auth;
using UptimeMonitor.Web.Api.Data;
using UptimeMonitor.Web.Data;

namespace UptimeMonitor.Web.Controllers.Api.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/workers")]
    [EndpointGroupName("Admin")]
    public class WorkerController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly MonitorDbContext _dbContext;

        public WorkerController(IMapper mapper, MonitorDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpGet("statistics")]
        [EndpointName("GetWorkerStatistics")]
        public async Task<ActionResult<GetWorkerStatisticsResponse>> GetStatisticsAsync()
        {
            var checkinCutoff = DateTime.UtcNow.AddMinutes(-10);

            var totalWorkers = await _dbContext.WorkerConfigurations.CountAsync();
            var activeWorkers = await _dbContext.WorkerConfigurations.Where(w => w.LastCheckIn > checkinCutoff).CountAsync();

            return Ok(new GetWorkerStatisticsResponse
            {
                TotalWorkers = totalWorkers,
                ActiveWorkers = activeWorkers
            });
        }

        [HttpGet]
        [EndpointName("GetWorkerList")]
        public async Task<ActionResult<PagedListResponse<WorkerConfiguration>>> GetListAsync(int page = 0, int pageSize = 0, string? search = null)
        {
            var query = _dbContext.WorkerConfigurations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Name.Contains(search));

            var configs = await query
                .OrderBy(w => w.Name).ThenBy(w => w.Location)
                .Paginate(pageSize, page).ToListAsync();

            var totalRecords = await query.CountAsync();
            var totalPages = (pageSize == 0) ? 1 : (totalRecords + pageSize - 1) / pageSize;

            return Ok(new PagedListResponse<WorkerConfiguration>
            {
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                Records = configs.Select(_mapper.Map<WorkerConfiguration>)
            });
        }

        [HttpPost]
        [EndpointName("CreateWorker")]
        public async Task<ActionResult<WorkerConfiguration>> CreateAsync(CreateWorkerRequest dto)
        {
            var worker = new Core.Domain.Workers.WorkerConfiguration()
            {
                Name = dto.Name,
                Location = dto.Location,
                ConfigUpdateInterval = dto.ConfigUpdateInterval,
                Secret = Cryptography.HashSecret(dto.Secret),
                Approved = true
            };

            await _dbContext.WorkerConfigurations.AddAsync(worker);
            await _dbContext.SaveChangesAsync();

            return base.Ok(_mapper.Map<WorkerConfiguration>(worker));
        }

        [HttpGet("{id}")]
        [EndpointName("GetWorkerById")]
        public async Task<ActionResult<WorkerConfiguration>> GetAsync(int id)
        {
            var worker = await _dbContext.WorkerConfigurations.FindAsync(id);

            if (worker == null)
            {
                return NotFound();
            }

            return base.Ok(_mapper.Map<WorkerConfiguration>(worker));
        }

        [HttpPut("{id}")]
        [EndpointName("UpdateWorker")]
        public async Task<ActionResult<WorkerConfiguration>> UpdateAsync(int id, UpdateWorkerRequest data)
        {
            var worker = await _dbContext.WorkerConfigurations.FindAsync(id);

            if (worker == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(data.Name) && worker.Name != data.Name)
            {
                worker.Name = data.Name;
            }

            if (!string.IsNullOrWhiteSpace(data.Location) && worker.Location != data.Location)
            {
                worker.Location = data.Location;
            }

            if (!string.IsNullOrWhiteSpace(data.Secret))
            {
                worker.Location = Cryptography.HashSecret(data.Secret);
            }

            if (worker.ConfigUpdateInterval != data.ConfigUpdateInterval)
            {
                worker.ConfigUpdateInterval = data.ConfigUpdateInterval;
            }

            await _dbContext.SaveChangesAsync();

            return base.Ok(_mapper.Map<WorkerConfiguration>(worker));
        }

        [HttpDelete("{id}")]
        [EndpointName("DeleteWorker")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var worker = await _dbContext.WorkerConfigurations.FindAsync(id);

            if (worker == null)
            {
                return NotFound();
            }

            _dbContext.WorkerConfigurations.Remove(worker);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
