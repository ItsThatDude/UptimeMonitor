using AutoMapper;
using UptimeMonitor.Core.Contracts.Api.Monitors;
using UptimeMonitor.Core.Plugins;

namespace UptimeMonitor.Web.AutoMapper
{
    public class PluginProfile : Profile
    {
        public PluginProfile()
        {
            CreateMap<MonitorPlugin, MonitorPluginDto>();
        }
    }
}
