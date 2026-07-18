using System;
using System.IO;
using Xunit;
using FileSystemCommands;

namespace task08tests;

public class FileSystemCommandsTests
{
    [Fact]
    public void DirectorySizeCommand_Constructor_ThrowsArgumentNullException_WhenPathIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DirectorySizeCommand(null!));
    }

    [Fact]
    public void FindFilesCommand_Constructor_ThrowsArgumentNullException_WhenPathOrPatternIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FindFilesCommand(null!, "*.txt"));
        Assert.Throws<ArgumentNullException>(() => new FindFilesCommand("some/path", null!));
    }

    [Fact]
    public void DirectorySizeCommand_ShouldCalculateSize()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "test1.txt"), "Hello");
        File.WriteAllText(Path.Combine(testDir, "test2.txt"), "World");

        var command = new DirectorySizeCommand(testDir);
        command.Execute(); 
        Assert.Equal(10, command.Size);     
        Assert.Equal(2, command.Files.Count);   
        Assert.Contains(command.Files, a => a.Name == "test1.txt");
        Assert.Contains(command.Files, b => b.Name == "test2.txt");

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void FindFilesCommand_ShouldFindMatchingFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file1.txt"), "Text");
        File.WriteAllText(Path.Combine(testDir, "file2.log"), "Log");

        var command = new FindFilesCommand(testDir, "*.txt");
        command.Execute();
        Assert.Single(command.Files, c => c.Name == "file1.txt");
        Directory.Delete(testDir, true);
    }

    [Fact]
    public void DirectorySizeCommand_TestFilesFromSubDirectory()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        var dir1 = Path.Combine(testDir, "Subfile1");
        Directory.CreateDirectory(dir1);
        var dir2 = Path.Combine(testDir, "Subfile1", "Subfile2");
        Directory.CreateDirectory(dir2);
        File.WriteAllText(Path.Combine(testDir, "file1.doc"), "Hello World");    
        File.WriteAllText(Path.Combine(dir1, "file2.txt"), "1234567890");     
        File.WriteAllText(Path.Combine(dir2, "file3.doc"), "Document");          
        File.WriteAllText(Path.Combine(dir2, "file4.txt"), "README");      

        var command = new DirectorySizeCommand(testDir);
        command.Execute();
        Assert.Equal(35, command.Size);          
        Assert.Equal(4, command.Files.Count);     
        Directory.Delete(testDir, true);
    }

    [Fact]
    public void FindFilesCommand_TestFilesFromSubDirectory()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        var dir1 = Path.Combine(testDir, "Subfile1");
        Directory.CreateDirectory(dir1);
        var dir2 = Path.Combine(testDir, "Subfile1", "Subfile2");
        Directory.CreateDirectory(dir2);
        File.WriteAllText(Path.Combine(testDir, "file1.doc"), "Hello World");    
        File.WriteAllText(Path.Combine(dir1, "file2.txt"), "1234567890");     
        File.WriteAllText(Path.Combine(dir2, "file3.doc"), "Document");          
        File.WriteAllText(Path.Combine(dir2, "file4.txt"), "README");

        var command = new FindFilesCommand(testDir, "*.doc");
        command.Execute();
        Assert.Equal(2, command.Files.Count);
        Assert.Single(command.Files, a => a.Name == "file1.doc");
        Assert.Single(command.Files, b => b.Name == "file3.doc");
        Directory.Delete(testDir, true);
    }
    
    [Fact]
    public void Console_TestOutput()
    {
        using var output = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(output);

        try
        {
            CommandRunner.Program.Main();
            var result = output.ToString();

            Assert.Contains("Размер всех файлов составляет 25 байт", result);
            Assert.Contains("Количество файлов с маской *.txt равно 2:", result);
            Assert.Contains("file1.txt", result);
            Assert.Contains("file3.txt", result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
