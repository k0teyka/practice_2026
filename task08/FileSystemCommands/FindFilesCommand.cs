using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLib;

namespace FileSystemCommands;

public class FindFilesCommand : ICommand
{
    private readonly string _directoryPath;
    private readonly string _searchPattern;

    public List<FileInfo> Files { get; private set; } = new();

    public FindFilesCommand(string directoryPath, string searchPattern)
    {
        _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        _searchPattern = searchPattern ?? throw new ArgumentNullException(nameof(searchPattern));
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
            var foundFiles = dirInfo.EnumerateFiles(_searchPattern, SearchOption.AllDirectories).ToList();

            Files = foundFiles;

            Console.WriteLine($"Количество файлов с маской {_searchPattern} равно {Files.Count}:");
            foreach (var file in Files)
            {
                Console.WriteLine(file.Name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при поиске файлов: {ex.Message}");
        }
    }
}
