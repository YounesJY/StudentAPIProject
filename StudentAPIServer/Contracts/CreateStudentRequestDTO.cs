using System.ComponentModel.DataAnnotations;

namespace Contracts
{
    public class CreateStudentRequestDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string Name { get; set; }

        [Range(1, 150)]
        public int Age { get; set; }

        [Range(0, 100)]
        public int Grade { get; set; }
    }
}
