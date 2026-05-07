namespace UptimeMonitor.Web.Config
{
    public class WorkerAuthOptions
    {
        public string Secret { get; set; } = "";
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
    }
}