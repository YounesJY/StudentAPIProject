using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class StudentResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }

        public StudentResponseDTO(int id, string name, int age, int grade, string? email, string? role)
        {
            Id = id;
            Name = name;
            Age = age;
            Grade = grade;
            Email = email;
            Role = role;
        }
    }
}
