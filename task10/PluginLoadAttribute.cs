namespace PluginLoader;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PluginLoadAttribute : Attribute
{
    public string Name { get; }
    public string[] Dependencies { get; }

    public PluginLoadAttribute(string name, params string[] dependencies)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Dependencies = dependencies ?? Array.Empty<string>();
    }
}
