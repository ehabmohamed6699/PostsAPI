using System.ComponentModel.DataAnnotations;

namespace PostsAPI.Models
{
    // DTO for auth requests (users send email and password for both login and register)
    public class AuthRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
