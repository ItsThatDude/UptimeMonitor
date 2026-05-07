using AutoMapper;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Contracts.Api.StatusPages;
using UptimeMonitor.Core.Domain.Monitors;
using UptimeMonitor.Core.Domain.StatusPages;
using UptimeMonitor.Web.Api.Data;

namespace UptimeMonitor.Web.Api.AutoMapper
{
    public class StatusPageProfile : Profile
    {
        public StatusPageProfile()
        {
            CreateMap<StatusPage, StatusPageSimpleDto>();
            CreateMap<StatusPage, StatusPageConfigResponse>();
            CreateMap<Core.Domain.StatusPages.StatusPageMonitorGroup, StatusPageGroupConfigDto>();

            CreateMap<StatusPage, GetStatusPageResponse>();

            CreateMap<Core.Domain.StatusPages.StatusPageMonitorGroup, StatusPageMonitorGroupDto>();

            CreateMap<StatusPageMonitorGroupMonitor, MonitorSimpleDto>()
                .ForMember(dest => dest.Id, mapper => mapper.MapFrom(src => src.Monitor.Id))
                .ForMember(dest => dest.Name, mapper => mapper.MapFrom(src => src.Monitor.Name))
                .ForMember(dest => dest.Type, mapper => mapper.MapFrom(src => src.Monitor.Type))
                .ForMember(dest => dest.Target, mapper => mapper.MapFrom(src => src.Monitor.Target));

            CreateMap<StatusPageMonitorGroupMonitor, StatusPageMonitorDto>()
                .ForMember(dest => dest.Id, mapper => mapper.MapFrom(src => src.Monitor.Id))
                .ForMember(dest => dest.Name, mapper => mapper.MapFrom(src => src.Monitor.Name));

            CreateMap<MonitorConfiguration, StatusPageMonitorDto>();
            CreateMap<MonitorResultEntry, HeartbeatDto>();
        }
    }
}
