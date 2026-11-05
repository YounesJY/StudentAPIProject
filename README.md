# StudentAPI — Secure 3-Tier Web API

A full 3-tier ASP.NET Core Web API backed by SQL Server, with JWT authentication, role-based authorization, and production-hardening patterns. Built as a learning project on top of the Programming Advices roadmap (Levels 25–26).

## Architecture

- **API Layer** — ASP.NET Core controllers, HTTP endpoints, JWT middleware, Swagger
- **Business Layer** — domain logic, validation, `SaveResult` / `DeleteResult` outcomes
- **Data Access Layer** — ADO.NET, stored procedures against SQL Server
- **Contracts** — shared DTOs (the shapes crossing boundaries); internal-only `StudentCredentialsDTO` never leaves the server

## Endpoints

### Auth

| Method | Route | Access |
|---|---|---|
| POST | `/api/Auth/login` | Public |
| POST | `/api/Auth/refresh` | Public (requires valid refresh token) |
| POST | `/api/Auth/logout` | Public (requires valid refresh token) |

### Students

| Method | Route | Access |
|---|---|---|
| GET | `/api/Students/All` | 🔒 Admin only |
| GET | `/api/Students/Passed` | 🌐 Public |
| GET | `/api/Students/AverageGrade` | 🌐 Public |
| GET | `/api/Students/{id}` | 🔒 Authenticated |
| POST | `/api/Students` | 🔒 Admin only |
| PUT | `/api/Students/{id}` | 🔒 Admin only |
| DELETE | `/api/Students/{id}` | 🔒 Admin only |

📌 `GET /api/Students/{id}` is authenticated-only at this stage. Ownership checks are planned for the next iteration.

## Security

- **JWT bearer authentication** — issued by `/api/Auth/login`, validated by middleware. `ClockSkew = TimeSpan.Zero`.
- **Refresh tokens** — 64-byte random, hashed with **SHA-256** before storage (not BCrypt — BCrypt's 72-byte limit truncates long tokens). Rotated on every refresh.
- **Password hashing** — BCrypt with automatic salt.
- **Role-based authorization** — `[Authorize(Roles = "Admin")]` on write endpoints and admin-only reads.
- **Swagger Bearer integration** — use the Authorize button to test protected endpoints.
- **Secrets** — connection string and JWT signing key are read from configuration (`appsettings.json` + `appsettings.Development.json` for local). Never hardcoded.

## Stack

- .NET 8
- ASP.NET Core Web API
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`
- `BCrypt.Net-Next`
- ADO.NET (`Microsoft.Data.SqlClient`)
- SQL Server 2022
- Swashbuckle (Swagger)

## Setup

1. Create a database named `StudentsDB` in SQL Server.
2. Run the stored procedures (see `Database/` folder).
3. Create `appsettings.Development.json` in the API project with:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=localhost;Database=StudentsDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
     },
     "JwtSettings": {
       "SecretKey": "YOUR_LONG_RANDOM_SECRET"
     }
   }
