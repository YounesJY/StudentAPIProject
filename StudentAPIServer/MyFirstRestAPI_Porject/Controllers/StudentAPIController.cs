using Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using StudentAPIBusinessLayer;


namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/Students")]
    public class StudentsController : ControllerBase
    {
        // ---------- GET /api/Students/All ----------
        [HttpGet("All", Name = "GetAllStudents")]
        [ProducesResponseType(typeof(IEnumerable<StudentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [HttpGet("Passed", Name = "GetPassedStudents")]
        [ProducesResponseType(typeof(IEnumerable<StudentResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<StudentResponseDTO>> GetPassedStudents()
        {
            try
            {
                List<StudentResponseDTO> passedStudents = Student.GetPassedStudents();

                if (passedStudents.Count == 0)
                    return NotFound("No students found.");

                return Ok(passedStudents);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        // ---------- GET /api/Students/AverageGrade ----------
        [HttpGet("AverageGrade", Name = "GetAverageGrade")]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<double> GetAverageGrade()
        {
            try
            {
                double averageGrade = Student.GetAverageGrade();
                return Ok(averageGrade);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        // ---------- GET /api/Students/{id} ----------
        [HttpGet("{id}", Name = "GetStudentById")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentResponseDTO> GetStudentById(int id)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            try
            {
                var student = Student.Find(id);

                if (student == null)
                    return NotFound($"Student with ID {id} not found.");

                return Ok(student.ToDTO());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        // ---------- POST /api/Students ----------
        [HttpPost(Name = "AddStudent")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(typeof(StudentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentResponseDTO> UpdateStudent(int id, [FromBody] UpdateStudentRequestDTO updatedStudent)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            if (updatedStudent == null)
                return BadRequest("Request body is required.");

            try
            {
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
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        // ---------- DELETE /api/Students/{id} ----------
        [HttpDelete("{id}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

