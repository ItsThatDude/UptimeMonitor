namespace UptimeMonitor.Core.Contracts.Api.Workers
{
    public class GetWorkerStatisticsResponse
    {
        public int TotalWorkers { get; set; }
        public int ActiveWorkers { get; set; }
    }
}
