using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pr16_v2_Khokhlov
{
    public class Student
    {
        public string LastName { get; set; }
        public string Group { get; set; }
        public string Specialty { get; set; }
        public string Discipline { get; set; }
        public int Grade { get; set; }
        private static ObservableCollection<Student> allStudents = new ObservableCollection<Student>();
        private static ObservableCollection<Student> displayedStudents = new ObservableCollection<Student>();
        public static ObservableCollection<Student> DisplayedStudents => displayedStudents;
        public static int TotalCount => allStudents.Count;
        public static double AverageGrade => allStudents.Count > 0 ? allStudents.Average(s => s.Grade) : 0;
        public static int ExcellentCount => allStudents.Count(s => s.Grade >= 4);
        public Student() { }

        public Student(string lastName, string group, string specialty, string discipline, int grade)
        {
            LastName = lastName;
            Group = group;
            Specialty = specialty;
            Discipline = discipline;
            Grade = grade;
        }
        public static void AddStudent(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            allStudents.Add(student);
            RefreshDisplay();
        }
        public static void UpdateStudent(Student oldStudent, Student newStudent)
        {
            if (oldStudent == null || newStudent == null)
                throw new ArgumentNullException();

            if (!string.IsNullOrEmpty(newStudent.LastName))
                oldStudent.LastName = newStudent.LastName;

            if (!string.IsNullOrEmpty(newStudent.Group))
                oldStudent.Group = newStudent.Group;

            if (!string.IsNullOrEmpty(newStudent.Specialty))
                oldStudent.Specialty = newStudent.Specialty;

            if (!string.IsNullOrEmpty(newStudent.Discipline))
                oldStudent.Discipline = newStudent.Discipline;

            if (newStudent.Grade >= 2 && newStudent.Grade <= 5)
                oldStudent.Grade = newStudent.Grade;

            RefreshDisplay();
        }
        public static void DeleteStudent(Student student)
        {
            if (student != null)
            {
                allStudents.Remove(student);
                RefreshDisplay();
            }
        }
        public static void ClearAllStudents()
        {
            allStudents.Clear();
            RefreshDisplay();
        }
        public static void LoadFromFile(string filePath)
        {
            var newStudents = new List<Student>();
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    if (isFirstLine && (line.Contains("Фамилия") || line.Contains("LastName")))
                    {
                        isFirstLine = false;
                        continue;
                    }
                    isFirstLine = false;
                    string[] parts = line.Split(',');

                    if (parts.Length >= 5)
                    {
                        string lastName = parts[0];
                        string group = parts[1];
                        string specialty = parts[2];
                        string discipline = parts[3];
                        int grade = 0;
                        int.TryParse(parts[4], out grade);
                        if (!string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(group))
                        {
                            newStudents.Add(new Student
                            {
                                LastName = lastName,
                                Group = group,
                                Specialty = specialty,
                                Discipline = discipline,
                                Grade = grade
                            });
                        }
                    }
                }
            }
            if (newStudents.Count > 0)
            {
                allStudents.Clear();
                foreach (var student in newStudents)
                {
                    allStudents.Add(student);
                }
                RefreshDisplay();
            }
        }
        public static void SaveToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("Фамилия,Группа,Специальность,Дисциплина,Оценка");
                foreach (var student in allStudents)
                {
                    writer.WriteLine($"{student.LastName},{student.Group},{student.Specialty},{student.Discipline},{student.Grade}");
                }
            }
        }
        public static void RefreshDisplay(string filterText = "", string sortBy = "")
        {
            var filtered = allStudents.AsEnumerable();

            if (!string.IsNullOrEmpty(filterText))
            {
                filtered = filtered.Where(s =>
                    s.LastName.ToLower().Contains(filterText) ||
                    s.Group.ToLower().Contains(filterText) ||
                    s.Specialty.ToLower().Contains(filterText) ||
                    s.Discipline.ToLower().Contains(filterText) ||
                    s.Grade.ToString().Contains(filterText));
            }
            switch (sortBy)
            {
                case "По фамилии (А-Я)":
                    filtered = filtered.OrderBy(s => s.LastName);
                    break;
                case "По фамилии (Я-А)":
                    filtered = filtered.OrderByDescending(s => s.LastName);
                    break;
                case "По группе":
                    filtered = filtered.OrderBy(s => s.Group).ThenBy(s => s.LastName);
                    break;
                case "По специальности":
                    filtered = filtered.OrderBy(s => s.Specialty).ThenBy(s => s.LastName);
                    break;
                case "По дисциплине":
                    filtered = filtered.OrderBy(s => s.Discipline).ThenBy(s => s.LastName);
                    break;
                case "По оценке (возрастание)":
                    filtered = filtered.OrderBy(s => s.Grade).ThenBy(s => s.LastName);
                    break;
                case "По оценке (убывание)":
                    filtered = filtered.OrderByDescending(s => s.Grade).ThenBy(s => s.LastName);
                    break;
                default:
                    filtered = filtered.OrderBy(s => s.LastName);
                    break;
            }
            displayedStudents.Clear();
            foreach (var student in filtered)
            {
                displayedStudents.Add(student);
            }
        }
        public static bool IsValid(Student student, out string errorMessage)
        {
            errorMessage = "";
            if (string.IsNullOrEmpty(student.LastName))
            {
                errorMessage = "Введите фамилию студента!";
                return false;
            }
            if (string.IsNullOrEmpty(student.Group))
            {
                errorMessage = "Введите группу студента!";
                return false;
            }
            if (string.IsNullOrEmpty(student.Specialty))
            {
                errorMessage = "Введите специальность студента!";
                return false;
            }
            if (string.IsNullOrEmpty(student.Discipline))
            {
                errorMessage = "Введите дисциплину!";
                return false;
            }
            if (student.Grade < 2 || student.Grade > 5)
            {
                errorMessage = "Оценка должна быть от 2 до 5!";
                return false;
            }
            return true;
        }
    }
}
