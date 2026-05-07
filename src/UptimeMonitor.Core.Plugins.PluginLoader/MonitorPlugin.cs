using System.Reflection;
using System.Runtime.Loader;

namespace UptimeMonitor.Core.Plugins
{
    public class MonitorPlugin
    {
        public Assembly Assembly { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public IEnumerable<Type> MonitorTypes { get; set; }
        public IDictionary<string, Type> MonitorTypeSettings { get; set; }
        public AssemblyLoadContext? LoadContext { get; set; }

        /// <summary>
        /// Attempt to unload the plugin's AssemblyLoadContext. This will only succeed if there are no remaining references
        /// to types/instances from the loaded assembly and the context is collectible.
        /// </summary>
        public void Unload()
        {
            try
            {
                LoadContext?.Unload();
            }
            catch
            {
                // Swallow - unload best-effort. Host should ensure no references remain.
            }
        }

        public MonitorPlugin(Assembly assembly, string filePath, IEnumerable<Type> monitorTypes, IDictionary<string, Type> monitorTypeSettings)
        {
            Assembly = assembly;
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            MonitorTypes = monitorTypes;
            MonitorTypeSettings = monitorTypeSettings;
        }
    }
}
