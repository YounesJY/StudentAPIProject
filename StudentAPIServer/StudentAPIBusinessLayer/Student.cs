using Contracts;
using StudentDataAccessLayer;



namespace StudentAPIBusinessLayer
{
    public class Student
    {
        public enum enMode { AddNew = 0, Update = 1 }

        public int ID { get; private set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }
        public enMode Mode { get; private set; }
        public StudentResponseDTO ToDTO()
        {
            return new StudentResponseDTO(ID, Name, Age, Grade);
        }


        private Student(StudentResponseDTO dto, enMode mode)
        {
            ID = dto.Id;
            Name = dto.Name;
            Age = dto.Age;
            Grade = dto.Grade;

            Mode = mode;
        }
        public Student(CreateStudentRequestDTO dto)
        {
            ID = 0;
            Name = dto.Name;
            Age = dto.Age;
            Grade = dto.Grade;

            Mode = enMode.AddNew;
        }


        public static Student? Find(int id)
        {
            StudentResponseDTO? dto = StudentData.GetStudentById(id);

            if (dto == null)
                return null;

            return new Student(dto, enMode.Update);
        }
        public static List<StudentResponseDTO> GetAllStudents()
        {
            return StudentData.GetAllStudents();
        }
        public static List<StudentResponseDTO> GetPassedStudents()
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