using Contracts.DTOs;
using Contracts.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAPIBusinessLayer;


namespace StudentApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Students")]
    public class StudentsController : ControllerBase
    {
        // ---------- GET /api/Students/All ----------
        [Authorize(Roles = "Admin")]
        [HttpGet("All", Name = "GetAllStudents")]
        [ProducesResponseType(typeof(IEnumerable<StudentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<StudentResponseDTO>> GetAllStudents()
        {
            try
            {
                List<StudentResponseDTO> students = Student.GetAllStudents();

                if (students.Count == 0)
                    return NotFound("No students found.");

                return Ok(students);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        // ---------- GET /api/Students/Passed ----------
        [AllowAnonymous]
        [HttpGet("Passed", Name = "GetPassedStudents")]
        [ProducesResponseType(typeof(IEnumerable<StudentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<StudentResponseDTO>> GetPassedStudents()
        {
            List<StudentResponseDTO> passedStudents = Student.GetPassedStudents();

            if (passedStudents.Count == 0)
                return NotFound("No students found.");

            return Ok(passedStudents);
        }

        // ---------- GET /api/Students/AverageGrade ----------
        [AllowAnonymous]
        [HttpGet("AverageGrade", Name = "GetAverageGrade")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult<double> GetAverageGrade()
        {
            double averageGrade = Student.GetAverageGrade();
            return Ok(averageGrade);
        }

        // ---------- GET /api/Students/{id} ----------
        [Authorize]
        [HttpGet("{id}", Name = "GetStudentById")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StudentResponseDTO>> GetStudentById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            var authResult = await authorizationService.AuthorizeAsync(User, id, "StudentOwnerOrAdmin");
            if (!authResult.Succeeded)
                return Forbid();

            var student = Student.Find(id);
            if (student == null)
                return NotFound($"Student with ID {id} not found.");

            return Ok(student.ToDTO());
        }

        // ---------- POST /api/Students ----------
        [Authorize(Roles = "Admin")]
        [HttpPost(Name = "AddStudent")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentResponseDTO> AddStudent([FromBody] CreateStudentRequestDTO newStudent)
        {
            if (newStudent == null)
                return BadRequest("Request body is required.");

            Student student = new Student(newStudent);
            SaveResult result = student.Save();

            return result.Status switch
            {
                SaveResult.enSaveResult.Success =>
                    CreatedAtRoute("GetStudentById", new { id = student.ID }, student.ToDTO()),

                SaveResult.enSaveResult.ValidationFailed =>
                    BadRequest(result.ErrorMessage),

                SaveResult.enSaveResult.DatabaseError =>
                    StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred."),

                _ =>
                    StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.")
            };
        }

        // ---------- PUT /api/Students/{id} ----------
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentResponseDTO> UpdateStudent(int id, [FromBody] UpdateStudentRequestDTO updatedStudent)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            if (updatedStudent == null)
                return BadRequest("Request body is required.");

            Student? student = Student.Find(id);
            if (student == null)
                return NotFound($"Student with ID {id} not found.");

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;

            SaveResult result = student.Save();

            return result.Status switch
            {
                SaveResult.enSaveResult.Success =>
                    Ok(student.ToDTO()),

                SaveResult.enSaveResult.ValidationFailed =>
                    BadRequest(result.ErrorMessage),

                SaveResult.enSaveResult.NotFound =>
                    NotFound(result.ErrorMessage),

                SaveResult.enSaveResult.DatabaseError =>
                    StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred."),

                _ =>
                    StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.")
            };
        }

        // ---------- DELETE /api/Students/{id} ----------
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteStudent")]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public ActionResult DeleteStudent(int id)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            DeleteResult result = Student.Delete(id);

            return result.Status switch
            {
                DeleteResult.enDeleteResult.Success =>
                    NoContent(),

                DeleteResult.enDeleteResult.NotFound =>
                    NotFound(result.ErrorMessage),

                DeleteResult.enDeleteResult.DatabaseError =>
                    StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred."),

                _ =>
                    StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.")
            };
        }
    }
}
