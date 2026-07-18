using System;
using System.IO;
using System.Reflection;

namespace MetadataRunner;

public static class AssemblyAnalyzer
{
    public static void Analyze(string dllPath, TextWriter writer)
    {
        if (string.IsNullOrWhiteSpace(dllPath))
            throw new ArgumentException("Путь к файлу библиотеки не может быть пустым.", nameof(dllPath));

        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"Файл сборки не найден по пути: {dllPath}");

        Assembly assembly;
        try
        {
            assembly = Assembly.LoadFrom(dllPath);
        }
        catch (Exception ex)
        {
            writer.WriteLine($"[Ошибка] Не удалось загрузить сборку: {ex.Message}");
            return;
        }

        writer.WriteLine($"Сборка: {assembly.GetName().Name}");
        writer.WriteLine(new string('=', 40));

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types ?? Array.Empty<Type>();
        }

        foreach (var type in types)
        {
            if (type == null) continue;

            writer.WriteLine($"Класс: {type.FullName}");

            var classAttributes = type.GetCustomAttributes();
            foreach (var attr in classAttributes)
            {
                writer.WriteLine($"  [Атрибут класса] {attr.GetType().Name}");
            }

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            if (constructors.Length > 0)
            {
                writer.WriteLine("  Конструкторы:");
                foreach (var ctor in constructors)
                {
                    writer.Write($"    - {type.Name}(");
                    var parameters = ctor.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.Write($"{parameters[i].ParameterType.Name} {parameters[i].Name}");
                        if (i < parameters.Length - 1) writer.Write(", ");
                    }
                    writer.WriteLine(")");
                }
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (methods.Length > 0)
            {
                writer.WriteLine("  Методы:");
                foreach (var method in methods)
                {
                    writer.Write($"    - {method.ReturnType.Name} {method.Name}(");
                    var parameters = method.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        writer.Write($"{parameters[i].ParameterType.Name} {parameters[i].Name}");
                        if (i < parameters.Length - 1) writer.Write(", ");
                    }
                    writer.WriteLine(")");

                    var methodAttrs = method.GetCustomAttributes();
                    foreach (var attr in methodAttrs)
                    {
                        writer.WriteLine($"      [Атрибут метода] {attr.GetType().Name}");
                    }
                }
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (properties.Length > 0)
            {
                writer.WriteLine("  Свойства:");
                foreach (var prop in properties)
                {
                    writer.WriteLine($"    - {prop.PropertyType.Name} {prop.Name}");
                    
                    var propAttrs = prop.GetCustomAttributes();
                    foreach (var attr in propAttrs)
                    {
                        writer.WriteLine($"      [Атрибут свойства] {attr.GetType().Name}");
                    }
                }
            }
            writer.WriteLine(new string('-', 30));
        }
    }
}
