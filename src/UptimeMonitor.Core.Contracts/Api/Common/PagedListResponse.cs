namespace UptimeMonitor.Core.Contracts.Api.Common
{
    public class PagedListResponse<T> where T : class
    {
        public required int TotalPages { get; set; }
        public required int TotalRecords { get; set; }
        public required IEnumerable<T> Records { get; set; }
    }
}
