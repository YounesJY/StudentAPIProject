using Contracts.DTOs;
using Contracts.DTOs.Auth;
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
        public string? Email { get; set; }
        public string? Role { get; set; }
        public enMode Mode { get; private set; }
        public StudentResponseDTO ToDTO()
        {
            return new StudentResponseDTO(ID, Name, Age, Grade, Email, Role);
        }


        private Student(StudentResponseDTO dto, enMode mode)
        {
            ID = dto.Id;
            Name = dto.Name;
            Age = dto.Age;
            Grade = dto.Grade;
            Email = dto.Email;
            Role = dto.Role;

            Mode = mode;
        }
        public Student(CreateStudentRequestDTO dto)
        {
            ID = 0;
            Name = dto.Name;
            Age = dto.Age;
            Grade = dto.Grade;
            Email = null;   
            Role = "Student"; 

            Mode = enMode.AddNew;
        }


        public static Student? Find(int id)
        {
            StudentResponseDTO? dto = StudentData.GetStudentById(id);

            if (dto == null)
                return null;

            return new Student(dto, enMode.Update);
        }
        public static StudentCredentialsDTO? FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return StudentData.GetStudentByEmail(email);
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
        public static DeleteResult Delete(int id)
        {
            if (id < 1)
                return DeleteResult.NotFound($"Student with ID {id} not found.");

            try
            {
                return StudentData.DeleteStudent(id)
                    ? DeleteResult.Success()
                    : DeleteResult.NotFound($"Student with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return DeleteResult.DatabaseError(ex.Message);
            }
        }


        public static bool SaveRefreshToken(int studentId, string refreshTokenHash, DateTime expiresAt)
        {
            return StudentData.UpdateRefreshToken(studentId, refreshTokenHash, expiresAt);
        }
        public static bool RevokeRefreshToken(int studentId)
        {
            return StudentData.RevokeRefreshToken(studentId);
        }

        private bool IsValid(out string? error)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                error = "Name cannot be empty.";
                return false;
            }

            if (Name.Length > 100)
            {
                error = "Name cannot exceed 100 characters.";
                return false;
            }

            if (Age < 0 || Age > 150)
            {
                error = "Age must be between 0 and 150.";
                return false;
            }

            if (Grade < 0 || Grade > 100)
            {
                error = "Grade must be between 0 and 100.";
                return false;
            }

            error = null;
            return true;
        }
        private SaveResult InsertNewStudent()
        {
            ID = StudentData.AddStudent(ToDTO());

            if (ID == -1)
                return SaveResult.DatabaseError("Failed to insert the student.");

            Mode = enMode.Update;
            return SaveResult.Success();
        }
        private SaveResult UpdateExistingStudent()
        {
            return StudentData.UpdateStudent(ToDTO())
                ? SaveResult.Success()
                : SaveResult.NotFound($"Student with ID {ID} not found.");
        }

        public SaveResult Save()
        {
            if (!IsValid(out string? error))
                return SaveResult.ValidationFailed(error!);

            try
            {
                return Mode switch
                {
                    enMode.AddNew => InsertNewStudent(),
                    enMode.Update => UpdateExistingStudent(),
                    _ => SaveResult.DatabaseError("Invalid entity mode.")
                };
            }
            catch (Exception ex)
            {
                return SaveResult.DatabaseError(ex.Message);
            }
        }
    }
}
