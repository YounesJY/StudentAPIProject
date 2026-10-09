using System.Data;
using Microsoft.Data.SqlClient;
using Contracts;


namespace StudentDataAccessLayer
{
    public class StudentData
    {
        private static readonly string _connectionString = DataAccesLayerSettings.GetConnectionString();

        private static StudentDTO MapToDTO(SqlDataReader reader)
        {
            return new StudentDTO(
                reader.GetInt32(reader.GetOrdinal("Id")),
                reader.GetString(reader.GetOrdinal("Name")),
                reader.GetInt32(reader.GetOrdinal("Age")),
                reader.GetInt32(reader.GetOrdinal("Grade"))
            );
        }

        public static List<StudentDTO> GetAllStudents()
        {
            var students = new List<StudentDTO>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SP_GetAllStudents", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(MapToDTO(reader));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while retrieving students from the database.", ex);
            }

            return students;
        }

        public static List<StudentDTO> GetPassedStudents()
        {
            var students = new List<StudentDTO>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SP_GetPassedStudents", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(MapToDTO(reader));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while retrieving passed students from the database.", ex);
            }

            return students;
        }

        public static double GetAverageGrade()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SP_GetAverageGrade", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    object result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return 0;

                    return Convert.ToDouble(result);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while calculating the average grade.", ex);
            }
        }

        public static StudentDTO? GetStudentById(int studentId)
        {
            if (studentId < 1)
                throw new ArgumentException("Student ID must be greater than zero.", nameof(studentId));

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SP_GetStudentById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapToDTO(reader);

                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"An error occurred while retrieving student with ID {studentId}.", ex);
            }
        }

        public static int AddStudent(StudentDTO student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            if (string.IsNullOrWhiteSpace(student.Name))
                throw new ArgumentException("Student name cannot be empty.", nameof(student));

            if (student.Age < 0)
                throw new ArgumentException("Student age cannot be negative.", nameof(student));

            if (student.Grade < 0)
                throw new ArgumentException("Student grade cannot be negative.", nameof(student));

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SP_AddStudent", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Name", student.Name);
                    command.Parameters.AddWithValue("@Age", student.Age);
                    command.Parameters.AddWithValue("@Grade", student.Grade);

                    var outputIdParam = new SqlParameter("@NewStudentId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputIdParam);

                    connection.Open();
                    command.ExecuteNonQuery();

                    if (outputIdParam.Value == null || outputIdParam.Value == DBNull.Value)
                        throw new Exception("Failed to retrieve the new student ID from the database.");

                    return (int)outputIdParam.Value;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while adding the student to the database.", ex);
            }
        }

        public static bool UpdateStudent(StudentDTO student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            if (student.Id < 1)
                throw new ArgumentException("Student ID must be greater than zero.", nameof(student));

            if (string.IsNullOrWhiteSpace(student.Name))
                throw new ArgumentException("Student name cannot be empty.", nameof(student));

            if (student.Age < 0)
                throw new ArgumentException("Student age cannot be negative.", nameof(student));

            if (student.Grade < 0)
                throw new ArgumentException("Student grade cannot be negative.", nameof(student));

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SP_UpdateStudent", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@StudentId", student.Id);
                    command.Parameters.AddWithValue("@Name", student.Name);
                    command.Parameters.AddWithValue("@Age", student.Age);
                    command.Parameters.AddWithValue("@Grade", student.Grade);

                    connection.Open();

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"An error occurred while updating student with ID {student.Id}.", ex);
            }
        }

        public static bool DeleteStudent(int studentId)
        {
            if (studentId < 1)
                throw new ArgumentException("Student ID must be greater than zero.", nameof(studentId));

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand("SP_DeleteStudent", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    connection.Open();

                    int rowsAffected = Convert.ToInt32(command.ExecuteScalar());
                    return rowsAffected > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"An error occurred while deleting student with ID {studentId}.", ex);
            }
        }
    }
}