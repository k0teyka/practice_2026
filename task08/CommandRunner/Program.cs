using System;
using System.IO;
using System.Reflection;
using CommandLib;

namespace CommandRunner;

public static class Program
{
    public static void Main()
    {
        string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSystemCommands.dll");

        if (!File.Exists(dllPath))
        {
            dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "FileSystemCommands", "bin", "Debug", "net10.0", "FileSystemCommands.dll");
            dllPath = Path.GetFullPath(dllPath);
        }

        Assembly? assembly = null;

        try
        {
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException($"Файл библиотеки не найден по пути: {dllPath}");
            }

            assembly = Assembly.LoadFrom(dllPath);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Файл не найден: {ex.Message}");
            return;
        }
        catch (BadImageFormatException ex)
        {
            Console.WriteLine($"Некорректный формат DLL или несовместимая архитектура: {ex.Message}");
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Нет прав доступа к файлу: {ex.Message}");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось загрузить сборку: {ex.Message}");
            return;
        }

        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "TestDirRunner");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);

            File.WriteAllText(Path.Combine(tempDir, "file1.txt"), "Hello World");
            File.WriteAllText(Path.Combine(tempDir, "file2.log"), "Log file content");
            var subDir = Path.Combine(tempDir, "Sub");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "file3.txt"), "1234567890");
            
            Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(tempDir, "file1.txt"), "1234567890");
            File.WriteAllText(Path.Combine(tempDir, "file2.log"), "12345");
            File.WriteAllText(Path.Combine(subDir, "file3.txt"), "1234567890");

            Type? sizeType = assembly.GetType("FileSystemCommands.DirectorySizeCommand");
            if (sizeType != null && typeof(ICommand).IsAssignableFrom(sizeType))
            {
                var sizeCmd = (ICommand?)Activator.CreateInstance(sizeType, tempDir);
                sizeCmd?.Execute();
            }

            Type? findType = assembly.GetType("FileSystemCommands.FindFilesCommand");
            if (findType != null && typeof(ICommand).IsAssignableFrom(findType))
            {
                var findCmd = (ICommand?)Activator.CreateInstance(findType, tempDir, "*.txt");
                findCmd?.Execute();
            }

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при инициализации или запуске команд: {ex.Message}");
        }
    }
}
