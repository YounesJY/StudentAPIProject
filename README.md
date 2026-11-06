
# StudentAPI — Secure 3-Tier Web API

A 3-tier ASP.NET Core Web API backed by SQL Server. JWT authentication, role-based and ownership-based authorization, rate limiting, audit logging. Built on the Programming Advices roadmap (Levels 25–26).

## Architecture

- **API Layer** — Controllers, HTTP endpoints, JWT middleware, Swagger.
- **Business Layer** — Domain logic, validation. Returns `SaveResult` / `DeleteResult`.
- **Data Access Layer** — ADO.NET, stored procedures against SQL Server.
- **Contracts** — Shared DTOs crossing layer boundaries. `StudentCredentialsDTO` is internal-only and never returned to clients.

## Request Pipeline

Middlewares execute in this order:

1. `UseExceptionHandler` — Catches unhandled exceptions from downstream layers.
2. `UseHttpsRedirection` — Enforces HTTPS.
3. `UseCors` — Applies `StudentApiCorsPolicy`.
4. `UseRateLimiter` — Applies `AuthLimiter` to `/login` and `/refresh`.
5. `UseAuthentication` — Parses JWT, populates `HttpContext.User`.
6. `UseAuthorization` — Enforces `[Authorize]`, roles, and policies.

Controllers are reached only after all six middlewares pass.

## Endpoints

### Auth

| Method | Route | Access | Notes |
|---|---|---|---|
| POST | `/api/Auth/login` | Public | Rate limited: 5 requests/minute per IP |
| POST | `/api/Auth/refresh` | Public | Requires valid refresh token. Rate limited: 5 requests/minute per IP |
| POST | `/api/Auth/logout` | Public | Requires valid refresh token. Idempotent |

### Students

| Method | Route | Access |
|---|---|---|
| GET | `/api/Students/All` | 🔒 Admin only |
| GET | `/api/Students/Passed` | 🌐 Public |
| GET | `/api/Students/AverageGrade` | 🌐 Public |
| GET | `/api/Students/{id}` | 🔒 Owner (Student) or Admin |
| POST | `/api/Students` | 🔒 Admin only |
| PUT | `/api/Students/{id}` | 🔒 Admin only |
| DELETE | `/api/Students/{id}` | 🔒 Admin only |

## Security

- **JWT bearer authentication.** Issued by `/api/Auth/login`, validated by middleware. `ClockSkew = TimeSpan.Zero` so tokens expire at `exp`.
- **Refresh tokens.** 64-byte random, hashed with SHA-256 before storage (BCrypt's 72-byte limit truncates long tokens). Rotated on every refresh. Logout is idempotent.
- **Password hashing.** BCrypt with automatic salt.
- **Role-based authorization.** `[Authorize(Roles = "Admin")]` on write endpoints and `/All`.
- **Ownership authorization.** `GET /api/Students/{id}` is restricted to the owner (matching JWT `NameIdentifier`) or Admin, enforced by a custom `IAuthorizationHandler`.
- **Rate limiting.** Fixed window, 5 requests/minute per IP on `/login` and `/refresh`. 429 responses return a JSON body via `OnRejected`.
- **Global exception handler.** Catches all unhandled exceptions, logs them server-side, returns `500 {"error":"An internal error occurred."}`. No stack traces or exception messages leak to clients.
- **Swagger Bearer integration.** Authorize button sends the JWT with protected requests.
- **Secrets.** Connection string and JWT signing key come from configuration (`appsettings.json` + `appsettings.Development.json` for local). Nothing hardcoded.

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
3. Create `appsettings.Development.json` in the API project:
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
4. Seed at least one admin account:
   ```sql
   UPDATE dbo.Students
   SET Role = 'Admin',
       Email = 'admin@example.com',
       PasswordHash = '<bcrypt hash>'
   WHERE Id = 1;
   ```
5. Run:
   ```bash
   dotnet run
   ```

## Testing auth

1. `POST /api/Auth/login` with valid credentials. Copy the `accessToken`.
2. In Swagger, click **Authorize** and paste the token.
3. Protected endpoints receive the token in the `Authorization` header.
4. Test refresh: `POST /api/Auth/refresh` with `{ "email": "...", "refreshToken": "..." }`. Response contains a new access token and a new refresh token. The old refresh token is invalidated.
5. Test logout: `POST /api/Auth/logout`. A second logout with the same token returns `200` without touching the DB.

### Test rate limiting

Send six login requests in a row:

```bash
for i in {1..6}; do
  curl -k -X POST "https://localhost:7217/api/Auth/login" \
       -H "Content-Type: application/json" \
       -d '{"email":"user.test@student.com","password":"wrong"}'
  echo ""
done
```

Expected:
- Requests 1–5: `401 Invalid credentials`
- Request 6: `429 {"error":"Too many login attempts. Please try again later."}`

Wait 60 seconds. The counter resets.

### Test ownership

Log in as a Student:
- `GET /api/Students/{ownId}` → `200`
- `GET /api/Students/{otherId}` → `403`

Log in as Admin:
- `GET /api/Students/{anyId}` → `200`

## Purpose

Learning project. Built to demonstrate full 3-tier separation of concerns, JWT authentication with refresh rotation, role-based and ownership-based authorization, rate limiting, and audit logging. Extracted and extended from Level 25 (Introduction to RESTful APIs) and Level 26 (Secure Your APIs) of the Programming Advices roadmap.