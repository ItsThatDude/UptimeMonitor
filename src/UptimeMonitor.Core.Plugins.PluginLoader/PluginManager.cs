using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;
using UptimeMonitor.Core.Plugins.Monitoring;
using UptimeMonitor.Core.Plugins.PluginBase.Monitoring;

namespace UptimeMonitor.Core.Plugins
{

    public class PluginManager
    {
        private readonly ILogger _logger;
        private List<MonitorPlugin> _plugins = new List<MonitorPlugin>();

        public IReadOnlyList<MonitorPlugin> Plugins => _plugins.AsReadOnly();

        public PluginManager(ILogger<PluginManager> logger) {
            _logger = logger;
        }

        public void LoadPlugins()
        {
            _plugins = GetPlugins().ToList();
        }

        public IEnumerable<MonitorPlugin> GetPlugins(Assembly assembly)
        {
            var plugins = new List<MonitorPlugin>();

            var assemblyName = assembly.GetName();

            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.LogError("ReflectionTypeLoadException occurred loading types from {assemblyLocation}", assembly.Location);

                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    _logger.LogError(loaderEx, "Loader exception");
                }

                types = ex.Types.Where(t => t != null)
                    .Select(t => t!).ToArray();
            }

            var monitorTypes = types.Where(t => typeof(IMonitorType).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            _logger.LogDebug($"Found {monitorTypes.Count()} Monitor Types in {assemblyName.Name}");

            if (monitorTypes.Count() == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                _logger.LogInformation(
                    $"Can't find any type which implements IMonitorType in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");

                return plugins;
            }

            var settingsTypes = new Dictionary<string, Type>();

            _logger.LogDebug($"Found {settingsTypes.Count()} Monitor Settings Types in {assemblyName.Name}");

            foreach (var type in monitorTypes)
            {
                var settingsType = types.Where(
                    t => t.IsClass && !t.IsAbstract && 
                    t.GetInterfaces()
                        .Any(i => i.IsGenericType && 
                                  i.GetGenericTypeDefinition() == typeof(IMonitorSettings<>) && 
                                  i.GetGenericArguments()[0] == type))
                    .FirstOrDefault();

                if(settingsType != null)
                {
                    settingsTypes.Add(type.Name, settingsType);
                }
            }

            plugins.Add(new MonitorPlugin(assembly, assembly.Location, monitorTypes, settingsTypes));
            _logger.LogInformation($"Added plugin {assemblyName.Name} from {assembly.Location} with {monitorTypes.Count()} monitor types and {settingsTypes.Count()} settings types");

            return plugins;
        }

        public IEnumerable<MonitorPlugin> GetPlugins()
        {
            _logger.LogInformation("Loading plugins...");

            var plugins = new List<MonitorPlugin>();
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(currentDir))
            {
                _logger.LogError("Failed to get current directory");

                return plugins;
            }

            var pluginDir = Path.Combine(currentDir, "plugins");

            if (!Directory.Exists(pluginDir))
            {
                _logger.LogWarning($"Plugin directory {pluginDir} does not exist");

                return plugins;
            }

            var dllFiles = Directory.EnumerateFiles(pluginDir, "*.Monitors.*.dll", SearchOption.AllDirectories);
            _logger.LogDebug($"Found {dllFiles.Count()} dll files in {pluginDir}");

            foreach (var dll in dllFiles)
            {
                _logger.LogDebug($"Scanning {dll} for Monitor Types");

                try
                {
                    var context = new PluginLoadContext(dll);
                    var assembly = context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dll)));

                    var discoveredPlugins = GetPlugins(assembly);

                    // Associate the load context with the discovered plugins so unload can be attempted later
                    foreach (var p in discoveredPlugins)
                    {
                        p.LoadContext = context;
                    }

                    plugins.AddRange(discoveredPlugins);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Failed to load assembly from dll {dll}");
                }
            }

            return plugins;
        }

        public MonitorPlugin? GetPlugin(string assemblyName)
        {
            // Accept either full name, simple assembly name, or file name
            return _plugins.FirstOrDefault(p =>
                string.Equals(p.Assembly.FullName, assemblyName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.Assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.FileName, assemblyName, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<Type> GetMonitorTypes()
        {
            return _plugins.SelectMany(p => p.MonitorTypes)
                .ToList();
        }

        public IDictionary<string,string> GetMonitorTypeNames()
        {
            var dictionary = new Dictionary<string,string>();

            try {
                var monitorTypes = _plugins.SelectMany(p => p.MonitorTypes);

                foreach (var type in monitorTypes)
                {
                    if (Activator.CreateInstance(type) is IMonitorType monitor)
                    {
                        if (!dictionary.ContainsKey(monitor.Name))
                        {
                            dictionary.Add(monitor.Name, monitor.DisplayName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An exception occurred getting the monitor types");
            }

            return dictionary;
        }

        public IDictionary<string, IEnumerable<SettingDefinition>> GetMonitorTypeSettingsSchemas()
        {
            var dictionary = new Dictionary<string, IEnumerable<SettingDefinition>>();

            try
            {
                var settingsTypes = _plugins.SelectMany(p => p.MonitorTypeSettings);

                foreach(var kvp in settingsTypes)
                {
                    var properties = new List<SettingDefinition>();

                    foreach(var property in kvp.Value.GetProperties())
                    {
                        var settingDefinitionAttribute = property.GetCustomAttribute<SettingDefinitionAttribute>();

                        if(settingDefinitionAttribute != null)
                        {
                            object? defaultValue = null;

                            var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                            if (defaultValueAttribute != null && defaultValueAttribute.Value != null)
                            {
                                defaultValue = defaultValueAttribute.Value;
                            }
                            
                            properties.Add(new SettingDefinition
                            {
                                Name = property.Name,
                                DisplayName = settingDefinitionAttribute.DisplayName,
                                Description = settingDefinitionAttribute.Description,
                                DataType = settingDefinitionAttribute.DataType,
                                Required = settingDefinitionAttribute.Required,
                                AllowMultiple = settingDefinitionAttribute.AllowMultiple,
                                DefaultValue = defaultValue
                            });
                        }
                    }

                    dictionary.Add(kvp.Key, properties);
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "An exception occurred getting the monitor type settings");
            }

            return dictionary;
        }

        /// <summary>
        /// Attempt to unload a plugin by assembly name (accepts full name, simple name or file name).
        /// This is best-effort: unload will only succeed if there are no remaining references to types
        /// from the plugin assembly.
        /// </summary>
        public bool UnloadPlugin(string assemblyName)
        {
            var plugin = GetPlugin(assemblyName);

            if (plugin == null)
                return false;

            try
            {
                plugin.Unload();
                _plugins = _plugins.Where(p => p != plugin).ToList();

                // Trigger a GC to help finalize and unload the collectible assembly load context
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                _logger.LogInformation("Unloaded plugin {plugin} successfully", assemblyName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to unload plugin {plugin}", assemblyName);
                return false;
            }
        }

        /// <summary>
        /// Attempt to unload all loaded plugins.
        /// </summary>
        public void UnloadAll()
        {
            foreach (var p in _plugins.ToList())
            {
                try
                {
                    p.Unload();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to unload plugin {plugin}", p.FileName);
                }
            }

            _plugins.Clear();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
