using Xunit;
using System.Linq;
using System.Collections.Generic;
using task02;

namespace task02tests;

public class StudentServiceTests
{
    private readonly List<Student> _testStudents;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _testStudents = new List<Student>
        {
            new() { Name = "Иван", Faculty = "ФИТ", Grades = new List<int> { 5, 4, 5 } },
            new() { Name = "Анна", Faculty = "ФИТ", Grades = new List<int> { 3, 4, 3 } },
            new() { Name = "Петр", Faculty = "Экономика", Grades = new List<int> { 5, 5, 5 } }
        };
        _service = new StudentService(_testStudents);
    }

    [Fact]
    public void GetStudentsByFaculty_ReturnsCorrectStudents()
    {
        var result = _service.GetStudentsByFaculty("ФИТ").ToList();
        
        Assert.Equal(2, result.Count);
        Assert.True(result.All(s => s.Faculty == "ФИТ"));
    }

    [Fact]
    public void GetFacultyWithHighestAverageGrade_ReturnsCorrectFaculty()
    {
        var result = _service.GetFacultyWithHighestAverageGrade();
        
        Assert.Equal("Экономика", result);
    }

    [Fact]
    public void GetStudentsWithMinAverageGrade_ReturnsCorrectStudents()
    {
        var result = _service.GetStudentsWithMinAverageGrade(4.2).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.True(result.All(s => s.Grades.Average() >= 4.2));
    }

    [Fact]
    public void GetStudentsOrderedByName_ReturnsCorrectList()
    {
        var result = _service.GetStudentsOrderedByName().ToList();
        var names = result.Select(s => s.Name).ToList();
        var expectedNames = new List<string> { "Анна", "Иван", "Петр" };
        
        Assert.Equal(expectedNames, names); 
    }

    [Fact]
    public void GroupStudentsByFaculty_ReturnsCorrectList()
    {
        var lookup = _service.GroupStudentsByFaculty();
        
        Assert.True(lookup.Contains("ФИТ"));
        Assert.True(lookup.Contains("Экономика"));
        
        var fitStudents = lookup["ФИТ"].Select(s => s.Name).ToList();
        var econStudents = lookup["Экономика"].Select(s => s.Name).ToList();

        Assert.Contains("Иван", fitStudents);
        Assert.Contains("Анна", fitStudents);
        Assert.Contains("Петр", econStudents);
        
        Assert.Equal(2, lookup["ФИТ"].Count());
        Assert.Equal(1, lookup["Экономика"].Count());
    }
}
