using System.Data;
using Microsoft.Data.SqlClient;
using Contracts.DTOs;

namespace StudentDataAccessLayer
{
    public class StudentData
    {
        private static readonly string _connectionString = DataAccesLayerSettings.GetConnectionString();
        private static StudentResponseDTO MapToResponseDTO(SqlDataReader reader)
        {
            return new StudentResponseDTO(
                reader.GetInt32(reader.GetOrdinal("Id")),
                reader.GetString(reader.GetOrdinal("Name")),
                reader.GetInt32(reader.GetOrdinal("Age")),
                reader.GetInt32(reader.GetOrdinal("Grade")),
                !reader.IsDBNull(reader.GetOrdinal("Email")) ? reader.GetString(reader.GetOrdinal("Email")) : null,
                !reader.IsDBNull(reader.GetOrdinal("Role")) ? reader.GetString(reader.GetOrdinal("Role")) : null
            );
        }


        public static List<StudentResponseDTO> GetAllStudents()
        {
            var students = new List<StudentResponseDTO>();

            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = new SqlCommand("SP_GetAllStudents", connection);
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(MapToResponseDTO(reader));
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while retrieving students from the database.", ex);
            }

            return students;
        }
        public static List<StudentResponseDTO> GetPassedStudents()
        {
            var students = new List<StudentResponseDTO>();

            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = new SqlCommand("SP_GetPassedStudents", connection);
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(MapToResponseDTO(reader));
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
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = new SqlCommand("SP_GetAverageGrade", connection);
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();

                object? result = command.ExecuteScalar();

                if (result is null or DBNull)
                    return 0;

                return Convert.ToDouble(result);
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while calculating the average grade.", ex);
            }
        }
        public static StudentResponseDTO? GetStudentById(int studentId)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = new SqlCommand("SP_GetStudentById", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;

                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return MapToResponseDTO(reader);

                return null;
            }
            catch (SqlException ex)
            {
                throw new Exception($"An error occurred while retrieving student with ID {studentId}.", ex);
            }
        }
        public static int AddStudent(StudentResponseDTO student)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = new SqlCommand("SP_AddStudent", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = student.Name;
                command.Parameters.Add("@Age", SqlDbType.Int).Value = student.Age;
                command.Parameters.Add("@Grade", SqlDbType.Int).Value = student.Grade;

                var newIdParam = new SqlParameter("@NewStudentId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(newIdParam);

                connection.Open();
                command.ExecuteNonQuery();

                if (newIdParam.Value is null or DBNull)
                    throw new Exception("Failed to retrieve the new student ID from the database.");

                return (int)newIdParam.Value;
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while adding the student to the database.", ex);
            }
        }
        public static bool UpdateStudent(StudentResponseDTO student)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = new SqlCommand("SP_UpdateStudent", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@StudentId", SqlDbType.Int).Value = student.Id;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = student.Name;
                command.Parameters.Add("@Age", SqlDbType.Int).Value = student.Age;
                command.Parameters.Add("@Grade", SqlDbType.Int).Value = student.Grade;

                var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParam);

                connection.Open();
                command.ExecuteNonQuery();

                return (int)returnParam.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception($"An error occurred while updating student with ID {student.Id}.", ex);
            }
        }
        public static bool DeleteStudent(int studentId)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                using SqlCommand command = new SqlCommand("SP_DeleteStudent", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@StudentId", SqlDbType.Int).Value = studentId;

                var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParam);

                connection.Open();
                command.ExecuteNonQuery();

                return (int)returnParam.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception($"An error occurred while deleting student with ID {studentId}.", ex);
            }
        }
    }
}