using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PostsAPI.Models
{
    // OG user model in database (password hash is what is stored not the password)
    public class User
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Role { get; set; } = "User";
        public string PasswordHash { get; set; } = string.Empty;

    }
}
