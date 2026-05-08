using System.Reflection;
using UptimeMonitor.Core.Plugins;
using Xunit;

namespace UptimeMonitor.Core.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void GetPlugins_ReturnsEmpty_WhenAssemblyHasNoMonitorTypes()
        {
            var manager = new PluginManager(new Microsoft.Extensions.Logging.Abstractions.NullLogger<PluginManager>());

            // Use mscorlib/System.Private.CoreLib assembly which won't contain IMonitorType implementations
            var assembly = typeof(object).Assembly;

            var plugins = manager.GetPluginsForAssembly(assembly);

            Assert.Empty(plugins);
        }
    }
}
