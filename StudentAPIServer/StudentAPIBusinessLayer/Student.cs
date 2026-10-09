using Contracts;
using StudentDataAccessLayer;



namespace StudentAPIBusinessLayer
{
    public class Student
    {
        public enum enMode { AddNew = 0, Update = 1 }

        public int ID { get; set; }        public string Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }

        public enMode Mode { get; private set; } = enMode.AddNew;
        public StudentDTO ToDTO()
        {
            return new StudentDTO(ID, Name, Age, Grade);
        }

        public Student(StudentDTO dto, enMode mode = enMode.AddNew)
        {
            ID = dto.Id;
            Name = dto.Name;
            Age = dto.Age;
            Grade = dto.Grade;

            Mode = mode;
        }

        public static Student? Find(int id)
        {
            StudentDTO? dto = StudentData.GetStudentById(id);

            if (dto == null)
                return null;

            return new Student(dto, enMode.Update);
        }
        public static List<StudentDTO> GetAllStudents()
        {
            return StudentData.GetAllStudents();
        }
        public static List<StudentDTO> GetPassedStudents()
        {
            return StudentData.GetPassedStudents();
        }
        public static double GetAverageGrade()
        {
            return StudentData.GetAverageGrade();
        }
        public static bool DeleteStudent(int id)
        {
            return StudentData.DeleteStudent(id);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (AddNewStudent())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return UpdateStudent();

                default:
                    return false;
            }
        }

        private bool AddNewStudent()
        {
            ID = StudentData.AddStudent(ToDTO());
            return ID != -1;
        }
        private bool UpdateStudent()
        {
            return StudentData.UpdateStudent(ToDTO());
        }
    }
}