using AutoMapper;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Domain.Monitors;

namespace UptimeMonitor.Web.Api.AutoMapper
{
    public class MonitorProfile : Profile
    {
        public MonitorProfile()
        {
            CreateMap<MonitorConfiguration, MonitorSimpleDto>();
            CreateMap<MonitorConfiguration, MonitorConfigurationResponse>();

            CreateMap<MonitorTlsDetails, MonitorTlsDetailsDto>();

            CreateMap<MonitorEvent, MonitorEventDto>();
        }
    }
}
