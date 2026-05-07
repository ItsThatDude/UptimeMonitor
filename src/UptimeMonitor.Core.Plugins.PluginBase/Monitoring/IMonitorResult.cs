
namespace UptimeMonitor.Core.Plugins.Monitoring
{
    public interface IMonitorResult
    {
        bool IsSuccessful { get; set; }
        string Message { get; set; }
        TimeSpan ResponseTime { get; set; }
    }
}