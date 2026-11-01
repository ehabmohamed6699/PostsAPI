using System.ComponentModel.DataAnnotations;

namespace PostsAPI.Models
{
    public class UpdatePostRequest
    {
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(500)]
        public string Content { get; set; }
    }
}
