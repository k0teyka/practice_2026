using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using task13;

namespace task13tests;

public class JsonManagerTests
{
    [Fact]
    public void TestSuccessSerializationAndDeserialization()
    {
        var student = new Student
        {
            FirstName = "Максим",
            LastName = "Иванов",
            BirthDate = new DateTime(2005, 5, 15),
            Grades = new List<Subject> { new Subject { Name = "Math", Grade = 5 } }
        };

        string json = JsonManager.SerializeStudent(student);
        var deserialized = JsonManager.DeserializeStudent(json);

        Assert.Equal(student.FirstName, deserialized.FirstName);
        Assert.Equal(student.LastName, deserialized.LastName);
        Assert.Single(deserialized.Grades);
        Assert.Equal("Math", deserialized.Grades[0].Name);
    }

    [Fact]
    public void TestValidationThrowsExceptionOnInvalidData()
    {
        string invalidJson = "{\"FirstName\":\"\",\"LastName\":\"Петров\",\"BirthDate\":\"2006-01-01T00:00:00\"}";

        Assert.Throws<Exception>(() => JsonManager.DeserializeStudent(invalidJson));
    }

    [Fact]
    public void TestSaveAndLoadFile()
    {
        var student = new Student
        {
            FirstName = "Олег",
            LastName = "Сидоров",
            BirthDate = new DateTime(2004, 10, 10)
        };
        string tempFile = Path.GetTempFileName();

        try
        {
            JsonManager.SaveToFile(tempFile, student);
            var loadedStudent = JsonManager.LoadFromFile(tempFile);

            Assert.Equal("Олег", loadedStudent.FirstName);
            Assert.Equal("Сидоров", loadedStudent.LastName);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
