using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Contracts.Api.System;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin/config")]
    [EndpointGroupName("Admin")]
    public class ConfigController : ControllerBase
    {
        private readonly MonitorDbContext _dbContext;

        public ConfigController(MonitorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [EndpointName("AdminGetSystemSettings")]
        public async Task<ActionResult<AdminSystemSettingsResponse>> GetAsync()
        {
            var defaultStatusPage = await _dbContext.StatusPages.FirstOrDefaultAsync(x => x.Default);

            return Ok(new AdminSystemSettingsResponse(
                DefaultStatusPageSlug: defaultStatusPage?.Slug ?? ""
            ));
        }

        [HttpPost]
        [EndpointName("AdminUpdateSystemSettings")]
        public async Task<ActionResult> UpdateAsync([FromBody] AdminUpdateSystemSettingsRequest request)
        {
            // If DefaultStatusPageSlug is provided, update the default status page
            if (request.DefaultStatusPageSlug != null)
            {
                if (string.IsNullOrWhiteSpace(request.DefaultStatusPageSlug))
                {
                    return BadRequest("DefaultStatusPageSlug cannot be empty.");
                }

                var defaultStatusPage = await _dbContext.StatusPages.FirstOrDefaultAsync(x => x.Default);

                // Unset previous default status page
                if (defaultStatusPage != null && defaultStatusPage.Slug != request.DefaultStatusPageSlug)
                {
                    defaultStatusPage.Default = false;
                    _dbContext.StatusPages.Update(defaultStatusPage);
                }

                // Set new default status page
                var newDefaultStatusPage = await _dbContext.StatusPages.FirstOrDefaultAsync(x => x.Slug == request.DefaultStatusPageSlug);
                if (newDefaultStatusPage != null && !newDefaultStatusPage.Default)
                {
                    newDefaultStatusPage.Default = true;
                    _dbContext.StatusPages.Update(newDefaultStatusPage);
                }
            }

            return Ok();
        }
    }
}
