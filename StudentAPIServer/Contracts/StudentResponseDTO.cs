using System.ComponentModel.DataAnnotations;

namespace Contracts
{
    public class StudentResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }

        public StudentResponseDTO(int id, string name, int age, int grade)
        {
            Id = id;
            Name = name;
            Age = age;
            Grade = grade;
        }
    }
}
