using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.System;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Controllers.Api
{
    [ApiController]
    [Route("api/config")]
    [EndpointGroupName("Public")]
    public class ConfigController : ControllerBase
    {
        private readonly MonitorDbContext _dbContext;

        public ConfigController(MonitorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        [EndpointName("GetSystemSettings")]
        public async Task<ActionResult<PublicSystemSettingsResponse>> GetAsync()
        {
            //var settings = await _dbContext.Settings.ToListAsync();
            var defaultStatusPage = await _dbContext.StatusPages.FirstOrDefaultAsync(x => x.Default);

            return Ok(new PublicSystemSettingsResponse(
                DefaultStatusPageSlug: defaultStatusPage?.Slug ?? ""
            ));
        }
    }
}
