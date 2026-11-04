using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs.Auth
{
    public class LogoutRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string RefreshToken { get; set; }
    }
}