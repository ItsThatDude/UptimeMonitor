using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Core.Domain.Workers;
using UptimeMonitor.Web.Api.Auth;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Api.Services
{
    public class WorkerService
    {
        private readonly MonitorDbContext _dbContext;

        public WorkerService(MonitorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsAsync(string workerId)
        {
            return await _dbContext.WorkerConfigurations.AnyAsync(w => w.Name == workerId);
        }

        public async Task RegisterWorker(string workerId, string location, string secret)
        {
            var existingWorker = await _dbContext.WorkerConfigurations.FirstOrDefaultAsync(w => w.Name == workerId);

            if (existingWorker == null)
            {
                var hashedSecret = Cryptography.HashSecret(secret);

                existingWorker = new WorkerConfiguration
                {
                    Name = workerId,
                    Location = location,
                    Secret = hashedSecret
                };

                await _dbContext.WorkerConfigurations.AddAsync(existingWorker);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
