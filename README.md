
- **API Layer** — ASP.NET Core controllers, HTTP endpoints
- **Business Layer** — domain logic, validation, Save/Find
- **Data Access Layer** — ADO.NET, stored procedures against SQL Server
- **Contracts** — shared DTOs (the shapes crossing boundaries)

## Endpoints

| Method | Route | Action |
|---|---|---|
| GET | /api/Students/All | List all students |
| GET | /api/Students/Passed | List students with Grade ≥ 50 |
| GET | /api/Students/AverageGrade | Average grade |
| GET | /api/Students/{id} | Get one student |
| POST | /api/Students | Add a new student |
| PUT | /api/Students/{id} | Update a student |
| DELETE | /api/Students/{id} | Delete a student |

## Stack

- .NET 8
- ASP.NET Core Web API
- ADO.NET (Microsoft.Data.SqlClient)
- SQL Server 2022
- Swashbuckle (Swagger)

## Setup

1. Create a database named `StudentsDB` in SQL Server.
2. Run the stored procedures (see `Database/` folder).
3. Update the connection string in `DataAccessLayerSettings.cs`.
4. `dotnet run` from the API project.

## Purpose

Learning project — building a full 3-tier API with proper separation of concerns.
