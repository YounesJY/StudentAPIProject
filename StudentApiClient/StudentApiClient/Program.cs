using System.Net.Http.Json;


namespace StudentApiClient
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }
    }

    class Program
    {
        static readonly HttpClient httpClient = new HttpClient();

        static async Task GetAllStudents()
        {
            try
            {
                Console.WriteLine("--------------------------");
                Console.WriteLine("Fetching all students.....");
                var students = await httpClient.GetFromJsonAsync<List<Student>>("api/Students/All");

                if (students != null)
                {
                    foreach (var student in students)
                        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static async Task GetPassedStudents()
        {
            try
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Fetching Passed students.....");
                var students = await httpClient.GetFromJsonAsync<List<Student>>("api/Students/Passed");

                if (students != null)
                {
                    foreach (var student in students)
                        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static async Task GetAverageGrade()
        {
            try
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Fetching average grade .....");
                var averageGrade = await httpClient.GetFromJsonAsync<float>("api/Students/AverageGrade");
                Console.WriteLine($"Average Grade: {averageGrade}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static async Task GetStudentById(int id)
        {
            try
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Fetching student with ID {id}..");

                var response = await httpClient.GetAsync($"api/Students/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var student = await response.Content.ReadFromJsonAsync<Student>();
                    if (student != null)
                        Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                    else
                        Console.WriteLine("Empty response body.");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error {(int)response.StatusCode}: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static async Task AddStudent(Student newStudent)
        {
            try
            {
                Console.WriteLine("-----------------------");
                Console.WriteLine("Adding a new student...");

                var response = await httpClient.PostAsJsonAsync("api/Students", newStudent);
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var location = response.Headers.Location;
                    var addedStudent = await response.Content.ReadFromJsonAsync<Student>();

                    if (addedStudent != null)
                    {
                        Console.WriteLine($"Created at: {location}");
                        Console.WriteLine($"ID: {addedStudent.Id}, Name: {addedStudent.Name}, Age: {addedStudent.Age}, Grade: {addedStudent.Grade}");
                    }
                    else
                        Console.WriteLine("Created, but response body was empty.");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error {(int)response.StatusCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static async Task DeleteStudent(int id)
        {
            try
            {
                Console.WriteLine("\n_____________________________");
                Console.WriteLine($"\nDeleting student with ID {id}...\n");

                var response = await httpClient.DeleteAsync($"api/Students/{id}");
                if (response.IsSuccessStatusCode)
                    Console.WriteLine($"Student with ID {id} has been deleted.");
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error {(int)response.StatusCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static async Task UpdateStudent(int id, Student updatedStudent)
        {
            try
            {
                Console.WriteLine("---------------------------------");
                Console.WriteLine($"Updating student with ID {id}...");

                var response = await httpClient.PutAsJsonAsync($"api/Students/{id}", updatedStudent);
                if (response.IsSuccessStatusCode)
                {
                    var student = await response.Content.ReadFromJsonAsync<Student>();
                    if (student != null)
                        Console.WriteLine($"Updated Student — ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, Grade: {student.Grade}");
                    else
                        Console.WriteLine("Update succeeded, but response body was empty.");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error {(int)response.StatusCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }


        static async Task Main(string[] args)
        {
            // [NEVER USE],
            // httpClient.BaseAddress = new Uri("http://localhost:5151/api/Students");
            // the base address should be set to the API's base URL, not a specific endpoint.
            // The correct way is to set it to "http://localhost:5151/api/" and then use relative paths for specific endpoints.

            // Set this to the correct URI for your API
            httpClient.BaseAddress = new Uri("http://localhost:5215/");

            await GetAllStudents();
            await GetPassedStudents();
            await GetAverageGrade();
            await GetStudentById(0);
            await GetStudentById(1);
            await GetStudentById(20);

            var newStudent = new Student
            {
                Name = "UnisJY",
                Age = 72,
                Grade = 75
            };
            await AddStudent(newStudent);
            await GetAllStudents();

            await DeleteStudent(2);
            await DeleteStudent(3);
            await DeleteStudent(4);
            await DeleteStudent(8);
            await GetAllStudents();

            await UpdateStudent(0, new Student { Id = 0, Name = "Salma", Age = 22, Grade = 90 });   // 400 — invalid ID
            await UpdateStudent(999, new Student { Id = 999, Name = "Akram", Age = 27, Grade = 60 }); // 404 — not found
            await UpdateStudent(1, new Student { Id = 1, Name = "Ala", Age = 42, Grade = 69 });   // 200 
            await GetAllStudents();
        }
    }
}
