namespace task02;

public class StudentService
{
    private readonly List<Student> _students;
    
    public StudentService(List<Student> students) => _students = students;

    public IEnumerable<Student> GetStudentsByFaculty(string faculty)
        => _students.Where(a => a.Faculty == faculty);

    public IEnumerable<Student> GetStudentsWithMinAverageGrade(double minAverageGrade) 
        => _students.Where(b => b.Grades != null && b.Grades.Average() >= minAverageGrade);

    public IEnumerable<Student> GetStudentsOrderedByName()
        => _students.OrderBy(c => c.Name);

    public ILookup<string, Student> GroupStudentsByFaculty()
        => _students.ToLookup(d => d.Faculty);

    public string GetFacultyWithHighestAverageGrade()
        => _students.GroupBy(e => e.Faculty).MaxBy(f => f.Average(g => g.Grades.Average()))!.Key;
}
