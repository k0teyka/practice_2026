using System.Reflection;

namespace PluginLoader;

public class PluginContainer
{
    private readonly Dictionary<string, Type> _pluginTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string[]> _dependencies = new(StringComparer.OrdinalIgnoreCase);

    public void DiscoverAndLoadPlugins(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        string[] dllFiles = Directory.GetFiles(directoryPath, "*.dll");

        foreach (var dll in dllFiles)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dll);
                FindPluginsInAssembly(assembly);
            }
            catch (Exception ex) when (ex is FileLoadException || ex is BadImageFormatException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"[Warning] Failed to load assembly '{dll}': {ex.Message}");
            }
        }
    }

    private void FindPluginsInAssembly(Assembly assembly)
    {
        try
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<PluginLoadAttribute>();
                if (attribute != null)
                {
                    MethodInfo? executeMethod = type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
                    if (executeMethod == null)
                    {
                        Console.WriteLine($"[Warning] Class '{type.FullName}' has [PluginLoad] but lacks a public 'Execute()' method.");
                        continue;
                    }

                    if (_pluginTypes.ContainsKey(attribute.Name))
                    {
                        throw new InvalidOperationException($"Duplicate plugin name detected: '{attribute.Name}'");
                    }

                    _pluginTypes[attribute.Name] = type;
                    _dependencies[attribute.Name] = attribute.Dependencies;
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine("[Warning] Some types could not be loaded from assembly.");
            foreach (var loaderException in ex.LoaderExceptions)
            {
                if (loaderException != null)
                {
                    Console.WriteLine($"  Loader Exception: {loaderException.Message}");
                }
            }
        }
    }

    public List<string> GetLoadOrder()
    {
        var loadOrder = new List<string>();
        var visited = new Dictionary<string, NodeState>(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in _pluginTypes.Keys)
        {
            visited[plugin] = NodeState.Unvisited;
        }

        foreach (var plugin in _pluginTypes.Keys)
        {
            if (visited[plugin] == NodeState.Unvisited)
            {
                Visit(plugin, visited, loadOrder);
            }
        }

        return loadOrder;
    }

    private void Visit(string plugin, Dictionary<string, NodeState> visited, List<string> loadOrder)
    {
        visited[plugin] = NodeState.Visiting;

        if (_dependencies.TryGetValue(plugin, out var deps))
        {
            foreach (var dep in deps)
            {
                if (!_pluginTypes.ContainsKey(dep))
                {
                    throw new InvalidOperationException($"Plugin '{plugin}' requires dependency '{dep}', which was not loaded.");
                }

                if (visited[dep] == NodeState.Visiting)
                {
                    throw new InvalidOperationException($"Circular dependency detected: '{plugin}' -> '{dep}'");
                }

                if (visited[dep] == NodeState.Unvisited)
                {
                    Visit(dep, visited, loadOrder);
                }
            }
        }

        visited[plugin] = NodeState.Visited;
        loadOrder.Add(plugin);
    }

    public void ExecutePlugins()
    {
        List<string> order = GetLoadOrder();

        foreach (var pluginName in order)
        {
            Type type = _pluginTypes[pluginName];
            
            try
            {
                object? instance = Activator.CreateInstance(type);
                if (instance == null) continue;

                MethodInfo? method = type.GetMethod("Execute");
                Console.WriteLine($"[Info] Launching plugin: {pluginName}...");
                method?.Invoke(instance, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to execute plugin '{pluginName}': {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }

    private enum NodeState
    {
        Unvisited,
        Visiting,
        Visited
    }
}
