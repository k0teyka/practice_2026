using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLib;

namespace FileSystemCommands;

public class DirectorySizeCommand : ICommand
{
    private readonly string _directoryPath;

    public long Size { get; private set; }
    public List<FileInfo> Files { get; private set; } = new();

    public DirectorySizeCommand(string directoryPath)
    {
        _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
    }

    public void Execute()
    {
        if (!Directory.Exists(_directoryPath))
        {
            Console.WriteLine($"Ошибка: Директория '{_directoryPath}' не существует.");
            return;
        }

        try
        {
            var dirInfo = new DirectoryInfo(_directoryPath);
            var allFiles = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).ToList();

            Files = allFiles;
            Size = allFiles.Sum(file => file.Length);

            Console.WriteLine($"Размер всех файлов составляет {Size} байт");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при вычислении размера каталога: {ex.Message}");
        }
    }
}
