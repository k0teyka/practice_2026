using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using PluginLoader;

namespace task10tests;

public class PluginContainerTests
{
    private class TestablePluginContainer : PluginContainer
    {
        private readonly Dictionary<string, Type> _mockPluginTypes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string[]> _mockDependencies = new(StringComparer.OrdinalIgnoreCase);

        public void AddMockPlugin(string name, string[] dependencies)
        {
            _mockPluginTypes[name] = typeof(TestablePluginContainer);
            _mockDependencies[name] = dependencies;
            
            var typeField = typeof(PluginContainer).GetField("_pluginTypes", BindingFlags.NonPublic | BindingFlags.Instance);
            var depField = typeof(PluginContainer).GetField("_dependencies", BindingFlags.NonPublic | BindingFlags.Instance);

            typeField?.SetValue(this, _mockPluginTypes);
            depField?.SetValue(this, _mockDependencies);
        }
    }

    [Fact]
    public void GetLoadOrder_ShouldOrderCorrectly_WhenNoCycles()
    {
        var container = new TestablePluginContainer();
        container.AddMockPlugin("PluginC", new[] { "PluginB" });
        container.AddMockPlugin("PluginB", new[] { "PluginA" });
        container.AddMockPlugin("PluginA", Array.Empty<string>());

        List<string> order = container.GetLoadOrder();

        Assert.Equal(3, order.Count);
        Assert.Equal("PluginA", order[0]);
        Assert.Equal("PluginB", order[1]);
        Assert.Equal("PluginC", order[2]);
    }

    [Fact]
    public void GetLoadOrder_ShouldThrowException_WhenCircularDependencyDetected()
    {
        var container = new TestablePluginContainer();
        container.AddMockPlugin("PluginA", new[] { "PluginB" });
        container.AddMockPlugin("PluginB", new[] { "PluginA" });

        var exception = Assert.Throws<InvalidOperationException>(() => container.GetLoadOrder());
        Assert.Contains("Circular dependency detected", exception.Message);
    }

    [Fact]
    public void GetLoadOrder_ShouldThrowException_WhenDependencyIsMissing()
    {
        var container = new TestablePluginContainer();
        container.AddMockPlugin("PluginA", new[] { "PluginX" });

        var exception = Assert.Throws<InvalidOperationException>(() => container.GetLoadOrder());
        Assert.Contains("requires dependency 'PluginX', which was not loaded", exception.Message);
    }
}
