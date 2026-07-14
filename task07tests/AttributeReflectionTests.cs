using System;
using System.IO;
using System.Reflection;
using Xunit;
using task07;

namespace task07tests;

public class AttributeReflectionTests
{
    [Fact]
    public void Class_HasDisplayNameAttribute()
    {
        var type = typeof(SampleClass);
        var attribute = type.GetCustomAttribute<DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Пример класса", attribute.DisplayName);
    }

    [Fact]
    public void Method_HasDisplayNameAttribute()
    {
        var method = typeof(SampleClass).GetMethod("TestMethod");
        Assert.NotNull(method);
        
        var attribute = method.GetCustomAttribute<DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Тестовый метод", attribute.DisplayName);
    }

    [Fact]
    public void Property_HasDisplayNameAttribute()
    {
        var prop = typeof(SampleClass).GetProperty("Number");
        Assert.NotNull(prop);

        var attribute = prop.GetCustomAttribute<DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Числовое свойство", attribute.DisplayName);
    }

    [Fact]
    public void Class_HasVersionAttribute()
    {
        var type = typeof(SampleClass);
        var attribute = type.GetCustomAttribute<VersionAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(1, attribute.Major);
        Assert.Equal(0, attribute.Minor);
    }

    [Fact]
    public void ReflectionHelper_PrintTypeInfo_OutputsCorrectData()
    {
        // Временно перехватываем консольный вывод
        using var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        try
        {
            ReflectionHelper.PrintTypeInfo(typeof(SampleClass));
            var output = sw.ToString();

            // Проверяем, что в консоль вывелось то, что нужно
            Assert.Contains("SampleClass - Пример класса", output);
            Assert.Contains("Version: 1.0", output);
            Assert.Contains("TestMethod - Тестовый метод", output);
            Assert.Contains("Number, Int32 - Числовое свойство", output);
        }
        finally
        {
            // Обязательно возвращаем стандартный вывод на место, чтобы не ломать другие тесты
            Console.SetOut(originalOut);
        }
    }
}
