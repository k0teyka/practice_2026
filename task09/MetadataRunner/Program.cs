using System;
using System.IO;

namespace MetadataRunner;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Ошибка: не указан путь к файлу библиотеки классов (DLL).");
            Console.WriteLine("Использование: MetadataRunner <путь_к_dll>");
            return;
        }

        string dllPath = args[0];

        try
        {
            AssemblyAnalyzer.Analyze(dllPath, Console.Out);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при анализе метаданных: {ex.Message}");
        }
    }
}
