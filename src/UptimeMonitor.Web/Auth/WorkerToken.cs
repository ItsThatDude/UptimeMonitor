namespace UptimeMonitor.Web.Api.Auth
{
    public class WorkerToken
    {
        public string Token { get; set; } = "";
        public DateTime Expires { get; set; } = DateTime.MinValue;
    }
}
