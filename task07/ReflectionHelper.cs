using System;
using System.Linq;
using System.Reflection;

namespace task07;

public static class ReflectionHelper 
{
    public static void PrintTypeInfo(Type type)
    {
        if (type == null) return;

        var classAttr = type.GetCustomAttribute<DisplayNameAttribute>();
        if (classAttr != null)
        {
            Console.WriteLine($"{type.Name} - {classAttr.DisplayName}");
        }

        var versionAttr = type.GetCustomAttribute<VersionAttribute>();
        if (versionAttr != null)
        {
            Console.WriteLine($"Version: {versionAttr.Major}.{versionAttr.Minor}");
        }

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<DisplayNameAttribute>() != null)
            .ToList();

        Console.WriteLine("Methods:");
        if (methods.Count > 0)
        {
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<DisplayNameAttribute>();
                if (attr != null)
                {
                    Console.WriteLine($"{method.Name} - {attr.DisplayName}");
                }
            }
        }
        else
        {
            Console.WriteLine("No methods");
        }

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(p => p.GetCustomAttribute<DisplayNameAttribute>() != null)
            .ToList();

        Console.WriteLine("Properties:");
        if (properties.Count > 0)
        {
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<DisplayNameAttribute>();
                if (attr != null)
                {
                    Console.WriteLine($"{property.Name}, {property.PropertyType.Name} - {attr.DisplayName}");
                }
            }
        }
        else
        {
            Console.WriteLine("No properties");
        }
    } 
}
