# StudentAPI — Secure 3-Tier Web API

A full 3-tier ASP.NET Core Web API backed by SQL Server, with JWT authentication, role-based authorization, and production-hardening patterns. Built as a learning project on top of the Programming Advices roadmap (Levels 25–26).

## Architecture

- **API Layer** — ASP.NET Core controllers, HTTP endpoints, JWT middleware, Swagger
- **Business Layer** — domain logic, validation, `SaveResult` / `DeleteResult` outcomes
- **Data Access Layer** — ADO.NET, stored procedures against SQL Server
- **Contracts** — shared DTOs (the shapes crossing boundaries); internal-only `StudentCredentialsDTO` never leaves the server

## Request Pipeline

Middlewares run in this order (top to bottom):

1. `UseExceptionHandler` — catches unhandled exceptions from every layer below
2. `UseHttpsRedirection` — forces HTTPS
3. `UseCors` — applies the `StudentApiCorsPolicy`
4. `UseRateLimiter` — applies the `AuthLimiter` policy to `/login` and `/refresh`
5. `UseAuthentication` — parses the JWT and populates `HttpContext.User`
6. `UseAuthorization` — enforces `[Authorize]`, roles, and policies

Controllers receive requests only after all six middlewares pass.


## Endpoints

### Auth

| Method | Route | Access | Notes |
|---|---|---|---|
| POST | `/api/Auth/login` | Public | Rate limited: 5 requests/minute per IP |
| POST | `/api/Auth/refresh` | Public (requires valid refresh token) | Rate limited: 5 requests/minute per IP |
| POST | `/api/Auth/logout` | Public (requires valid refresh token) | Not rate limited |

### Students

| Method | Route | Access |
|---|---|---|
| GET | `/api/Students/All` | 🔒 Admin only |
| GET | `/api/Students/Passed` | 🌐 Public |
| GET | `/api/Students/AverageGrade` | 🌐 Public |
| GET | `/api/Students/{id}` | 🔒 Authenticated (owner or Admin) |
| POST | `/api/Students` | 🔒 Admin only |
| PUT | `/api/Students/{id}` | 🔒 Admin only |
| DELETE | `/api/Students/{id}` | 🔒 Admin only |


## Security

- **JWT bearer authentication** — issued by `/api/Auth/login`, validated by middleware. `ClockSkew = TimeSpan.Zero` so tokens expire at the exact `exp` timestamp.
- **Refresh tokens** — 64-byte random, hashed with **SHA-256** before storage (not BCrypt — BCrypt's 72-byte limit truncates long tokens). Rotated on every refresh.
- **Password hashing** — BCrypt with automatic salt.
- **Role-based authorization** — `[Authorize(Roles = "Admin")]` on write endpoints and admin-only reads.
- **Ownership authorization** — `GET /api/Students/{id}` is restricted to the owner (matching JWT `NameIdentifier`) or Admin, enforced by a custom `IAuthorizationHandler`.
- **Rate limiting** — 5 requests/minute per IP on `/login` and `/refresh`, fixed window. 429 responses use a friendly JSON body via `OnRejected`.
- **Global exception handler** — catches all unhandled exceptions, logs them server-side, and returns a generic `500 {"error":"An internal error occurred."}` to the client. No exception details or stack traces leak to the response.
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
   ```
4. Seed at least one admin (SQL `UPDATE dbo.Students SET Role = 'Admin', ...`).
5. `dotnet run` from the API project.


## Testing auth

1. `POST /api/Auth/login` with a valid email and password — copy the returned `accessToken`.
2. Click **Authorize** in Swagger, paste the token.
3. Protected endpoints now include the token in the `Authorization` header.
4. To test refresh: `POST /api/Auth/refresh` with `{ "email": "...", "refreshToken": "..." }`. The response includes a new access token and a **new** refresh token (rotation).

### Test rate limiting

Hit `/api/Auth/login` 6 times in a row within one minute:

```bash
for i in {1..6}; do
  curl -k -X POST "https://localhost:7217/api/Auth/login" \
       -H "Content-Type: application/json" \
       -d '{"email":"user.test@student.com","password":"wrong"}'
  echo ""
done
```