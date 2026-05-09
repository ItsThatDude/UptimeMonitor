using System.Net;

namespace UptimeMonitor.Monitors.HttpMonitor
{
    public class HttpMonitorProxy : IWebProxy
    {
        public ICredentials? Credentials { get; set; }
        public Uri Destination { get; private set; }
        public List<Uri> BypassedHosts { get; private set; } = new List<Uri>();

        public HttpMonitorProxy(Uri destination, IEnumerable<Uri>? bypassedHosts = null)
        {
            Destination = destination;

            if (bypassedHosts != null)
            {
                BypassedHosts.AddRange(bypassedHosts);
            }
        }

        public Uri? GetProxy(Uri destination)
        {
            return Destination;
        }

        public bool IsBypassed(Uri host)
        {
            return BypassedHosts.Exists(h => h == host);
        }
    }

}