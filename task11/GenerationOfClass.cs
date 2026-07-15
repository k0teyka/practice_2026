using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace task11;

public static class GenerationOfClass
{
    public static ICalculator CreateCalculator(string sourceCode)
    {
        string modifiedCode = sourceCode.Replace("public class Calculator", "public class Calculator : ICalculator");
        string finalCode = "using task11;\n" + modifiedCode;

        var syntaxTree = CSharpSyntaxTree.ParseText(finalCode);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICalculator).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };

        var compilation = CSharpCompilation.Create(
            "MyAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var memoryStream = new MemoryStream();
        var result = compilation.Emit(memoryStream);

        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage());
            
            throw new Exception("Ошибка компиляции: " + string.Join("; ", errors));
        }

        var assembly = Assembly.Load(memoryStream.ToArray());
        var type = assembly.GetType("Calculator");
        var instance = Activator.CreateInstance(type!);

        return (ICalculator)instance!;
    }
}
