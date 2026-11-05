using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using StudentApi.Configuration;
using StudentAPIBusinessLayer;
using Contracts.DTOs.Auth;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IOptions<JwtSettings> jwtOptions, ILogger<AuthController> logger)
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }


        private static string HashRefreshToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
        private string GenerateAccessToken(StudentCredentialsDTO student)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
                new Claim(ClaimTypes.Email, student.Email),
                new Claim(ClaimTypes.Role, student.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var student = Student.FindByEmail(request.Email);

            if (student == null)
            {
                _logger.LogWarning(
                    "Failed login attempt (email not found). Email={Email}, IP={IP}",
                    request.Email, ip);
                return Unauthorized("Invalid credentials");
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash);
            if (!isValidPassword)
            {
                _logger.LogWarning(
                    "Failed login attempt (bad password). Email={Email}, IP={IP}",
                    request.Email, ip);
                return Unauthorized("Invalid credentials");
            }

            var accessToken = GenerateAccessToken(student);
            var refreshToken = GenerateRefreshToken();
            var refreshHash = HashRefreshToken(refreshToken);
            var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

            Student.SaveRefreshToken(student.Id, refreshHash, expiresAt);

            _logger.LogInformation(
                "Successful login. UserId={UserId}, Email={Email}, IP={IP}",
                student.Id, student.Email, ip);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var student = Student.FindByEmail(request.Email);

            if (student == null ||
                student.RefreshTokenHash == null ||
                student.RefreshTokenRevokedAt != null ||
                student.RefreshTokenExpiresAt == null ||
                student.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Invalid refresh token attempt. Email={Email}, IP={IP}",
                    request.Email, ip);
                return Unauthorized();
            }

            var incomingHash = HashRefreshToken(request.RefreshToken);
            if (incomingHash != student.RefreshTokenHash)
            {
                _logger.LogWarning(
                    "Invalid refresh token attempt. Email={Email}, IP={IP}",
                    request.Email, ip);
                return Unauthorized();
            }

            var newAccessToken = GenerateAccessToken(student);
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshHash = HashRefreshToken(newRefreshToken);
            var newExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

            Student.SaveRefreshToken(student.Id, newRefreshHash, newExpiresAt);

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            var student = Student.FindByEmail(request.Email);

            if (student == null || student.RefreshTokenHash == null)
                return Ok();

            var incomingHash = HashRefreshToken(request.RefreshToken);
            if (incomingHash != student.RefreshTokenHash)
                return Ok();

            Student.RevokeRefreshToken(student.Id);
            return Ok("Logged out successfully");
        }
    }
}