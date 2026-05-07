using AutoMapper;
using UptimeMonitor.Core.Contracts.Api.Workers;

namespace UptimeMonitor.Web.Api.AutoMapper
{
    public class WorkerProfile : Profile
    {
        public WorkerProfile()
        {
            CreateMap<Core.Domain.Workers.WorkerConfiguration, WorkerConfiguration>();
        }
    }
}
