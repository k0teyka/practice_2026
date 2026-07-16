using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace task13;

public static class JsonManager
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true 
    };

    public static string SerializeStudent(Student student)
    {
        return JsonSerializer.Serialize(student, Options);
    }

    public static Student DeserializeStudent(string json)
    {
        var student = JsonSerializer.Deserialize<Student>(json, Options);

        if (student == null)
        {
            throw new Exception("Не удалось десериализовать студента.");
        }

        if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
        {
            throw new Exception("Ошибка валидации: Имя или Фамилия не могут быть пустыми.");
        }

        if (student.BirthDate > DateTime.Now)
        {
            throw new Exception("Ошибка валидации: Дата рождения не может быть в будущем.");
        }

        return student;
    }

    public static void SaveToFile(string filePath, Student student)
    {
        string json = SerializeStudent(student);
        File.WriteAllText(filePath, json);
    }

    public static Student LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл {filePath} не найден.");
        }
        string json = File.ReadAllText(filePath);
        return DeserializeStudent(json);
    }
}
