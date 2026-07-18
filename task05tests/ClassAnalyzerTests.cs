using task05;
using Xunit;

namespace task05tests;

public class TestClass
{
    public int PublicField;
    private string _privateField = "test";
    
    public int Property { get; set; }

    public void Method() { }

    public void MethodWithParams(int count, string name) { }
}

[Serializable]
public class AttributedClass { }

public class ClassAnalyzerTests
{
    [Fact]
    public void GetPublicMethods_ReturnsCorrectMethods()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var methods = analyzer.GetPublicMethods();

        Assert.Contains("Method", methods);
    }

    [Fact]
    public void GetAllFields_IncludesPrivateFields()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var fields = analyzer.GetAllFields();

        Assert.Contains("_privateField", fields);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTypeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ClassAnalyzer(null!));
    }

    [Fact]
    public void GetMethodParams_ReturnsCorrectParameters()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var parameters = analyzer.GetMethodParams("MethodWithParams").ToList();

        Assert.Contains("Int32 count", parameters);
        Assert.Contains("String name", parameters);
    }

    [Fact]
    public void GetMethodParams_ReturnsEmpty_WhenMethodDoesNotExist()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var parameters = analyzer.GetMethodParams("NonExistentMethod");

        Assert.Empty(parameters);
    }

    [Fact]
    public void GetProperties_ReturnsCorrectProperties()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var properties = analyzer.GetProperties().ToList();

        Assert.Contains("Property", properties);
    }

    [Fact]
    public void HasAttribute_ReturnsTrue_WhenAttributeExists()
    {
        var analyzer = new ClassAnalyzer(typeof(AttributedClass));
        var hasAttribute = analyzer.HasAttribute<SerializableAttribute>();

        Assert.True(hasAttribute);
    }

    [Fact]
    public void HasAttribute_ReturnsFalse_WhenAttributeDoesNotExist()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var hasAttribute = analyzer.HasAttribute<SerializableAttribute>();

        Assert.False(hasAttribute);
    }
}
