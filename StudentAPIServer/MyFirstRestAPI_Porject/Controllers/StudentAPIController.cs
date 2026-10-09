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
        [ProducesResponseType(typeof(IEnumerable<StudentDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDTO>> GetAllStudents()
        {
            List<StudentDTO> students = Student.GetAllStudents();


            if (students.Count == 0)
                return NotFound("No students found.");

            return Ok(students);
        }

        // ---------- GET /api/Students/Passed ----------
        [HttpGet("Passed", Name = "GetPassedStudents")]
        [ProducesResponseType(typeof(IEnumerable<StudentDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDTO>> GetPassedStudents()
        {
            List<StudentDTO> passedStudents = Student.GetPassedStudents();

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
        [ProducesResponseType(typeof(StudentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public ActionResult<StudentDTO> GetStudentById(int id)
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
        [ProducesResponseType(typeof(StudentDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public ActionResult<StudentDTO> AddStudent(StudentDTO newStudent)
        {
            if (newStudent == null
                || string.IsNullOrEmpty(newStudent.Name)
                || newStudent.Age < 0
                || newStudent.Grade < 0)
            {
                return BadRequest("Invalid student data.");
            }

            Student student = new Student(newStudent);
            student.Save();

            // Reflect the server-assigned ID back on the DTO.
            newStudent.Id = student.ID;

            // 201 Created + Location header pointing at the new resource.
            return CreatedAtRoute("GetStudentById", new { id = newStudent.Id }, newStudent);
        }

        // ---------- PUT /api/Students/{id} ----------
        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(typeof(StudentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public ActionResult<StudentDTO> UpdateStudent(int id, StudentDTO updatedStudent)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            if (updatedStudent == null
                || string.IsNullOrEmpty(updatedStudent.Name)
                || updatedStudent.Age < 0
                || updatedStudent.Grade < 0)
            {
                return BadRequest("Invalid student data.");
            }

            var student = Student.Find(id);

            if (student == null)
                return NotFound($"Student with ID {id} not found.");

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;
            student.Save();

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