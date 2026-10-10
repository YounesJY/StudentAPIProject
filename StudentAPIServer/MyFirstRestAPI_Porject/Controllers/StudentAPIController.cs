using Microsoft.AspNetCore.Mvc;
using StudentAPIBusinessLayer;
using Contracts;


namespace StudentApi.Controllers
{
    [ApiController]
    // [Route("[controller]")] // Dynamic route — [controller] is replaced by the class name minus "Controller".
    // Renaming the class changes the URL and breaks external clients. Hard-coded for stability.
    [Route("api/Students")]
    public class StudentsController : ControllerBase
    {
        // ---------- GET /api/Students/All ----------
        [HttpGet("All", Name = "GetAllStudents")]
        [ProducesResponseType(typeof(IEnumerable<StudentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentResponseDTO>> GetAllStudents()
        {
            List<StudentResponseDTO> students = Student.GetAllStudents();


            if (students.Count == 0)
                return NotFound("No students found.");

            return Ok(students);
        }

        // ---------- GET /api/Students/Passed ----------
        [HttpGet("Passed", Name = "GetPassedStudents")]
        [ProducesResponseType(typeof(IEnumerable<StudentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentResponseDTO>> GetPassedStudents()
        {
            List<StudentResponseDTO> passedStudents = Student.GetPassedStudents();

            if (passedStudents.Count == 0)
                return NotFound("No students found.");

            return Ok(passedStudents);
        }

        // ---------- GET /api/Students/AverageGrade ----------
        [HttpGet("AverageGrade", Name = "GetAverageGrade")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        public ActionResult<double> GetAverageGrade()
        {
            double averageGrade = Student.GetAverageGrade();
            return Ok(averageGrade);
        }

        // ---------- GET /api/Students/{id} ----------
        [HttpGet("{id}", Name = "GetStudentById")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentResponseDTO> GetStudentById(int id)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            var student = Student.Find(id);

            if (student == null)
                return NotFound($"Student with ID {id} not found.");

            // Return the DTO, not the domain object.
            return Ok(student.ToDTO());
        }

        // ---------- POST /api/Students ----------
        [HttpPost(Name = "AddStudent")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public ActionResult<StudentResponseDTO> AddStudent(CreateStudentRequestDTO newStudent)
        {
            /*
                Factory method (Student.CreateNew) would read clearer at the call site,
                but the current public constructor achieves the same goal: the API can't
                pick the mode, only the BL can. The constructor sets Mode = AddNew
                internally. Swapping to a factory later is a two-line change.
            */
            Student student = new Student(newStudent);

            if (!student.Save())
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create student.");

            return CreatedAtRoute("GetStudentById", new { id = student.ID }, student.ToDTO());
        }

        // ---------- PUT /api/Students/{id} ----------
        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentResponseDTO> UpdateStudent(int id, UpdateStudentRequestDTO updatedStudent)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            Student? student = Student.Find(id);
            if (student == null)
                return NotFound($"Student with ID {id} not found.");

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;

            if (!student.Save())
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to update student with ID {id}.");

            return Ok(student.ToDTO());
        }

        // ---------- DELETE /api/Students/{id} ----------
        [HttpDelete("{id}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public ActionResult DeleteStudent(int id)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            if (Student.DeleteStudent(id))
                return NoContent();

            return NotFound($"Student with ID {id} not found.");
        }
    }
}