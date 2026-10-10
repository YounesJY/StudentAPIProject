using System.ComponentModel.DataAnnotations;

namespace Contracts
{
    public class StudentDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }

        [Range(1, 150)]
        public int Age { get; set; }

        [Range(0, 100)]
        public int Grade { get; set; }


        public StudentDTO(int id, string name, int age, int grade)
        {
            Id = id;
            Name = name;
            Age = age;
            Grade = grade;
        }
    }
}
