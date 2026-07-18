using System;
using System.IO;
using Xunit;
using MetadataRunner;
using task07;

namespace task09tests;

public class AssemblyAnalyzerTests
{
    [Fact]
    public void Analyzer_ThrowsArgumentException_WhenPathIsEmpty()
    {
        using var sw = new StringWriter();
        Assert.Throws<ArgumentException>(() => AssemblyAnalyzer.Analyze("", sw));
    }

    [Fact]
    public void Analyzer_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        using var sw = new StringWriter();
        Assert.Throws<FileNotFoundException>(() => AssemblyAnalyzer.Analyze("nonexistent_file.dll", sw));
    }

    [Fact]
    public void Analyzer_ShouldExtractMetadataAndAttributes_FromValidAssembly()
    {
        string task07DllPath = typeof(SampleClass).Assembly.Location;

        using var sw = new StringWriter();
        AssemblyAnalyzer.Analyze(task07DllPath, sw);
        
        string output = sw.ToString();

        Assert.Contains("Сборка: task07", output);
        Assert.Contains("Класс: task07.SampleClass", output);
        Assert.Contains("[Атрибут класса] DisplayNameAttribute", output);
        Assert.Contains("[Атрибут класса] VersionAttribute", output);
        
        Assert.Contains("TestMethod", output);
        Assert.Contains("[Атрибут метода] DisplayNameAttribute", output);
        Assert.Contains("Int32 Number", output);
        Assert.Contains("[Атрибут свойства] DisplayNameAttribute", output);
    }
}
