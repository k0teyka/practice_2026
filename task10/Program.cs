using System;

namespace PluginLoader;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Dynamic Plugin Loading System ===");

        if (args.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Usage: dotnet run -- <path_to_plugins_directory>");
            Console.ResetColor();
            return;
        }

        string pluginsDir = args[0];

        try
        {
            var container = new PluginContainer();
            Console.WriteLine($"Scanning directory: '{pluginsDir}'...");
            container.DiscoverAndLoadPlugins(pluginsDir);
            Console.WriteLine("\nExecuting plugins in dependency order...");
            container.ExecutePlugins();

            Console.WriteLine("\n=== Execution finished successfully ===");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[Fatal Error] Application execution failed: {ex.Message}");
            Console.ResetColor();
        }
    }
}
