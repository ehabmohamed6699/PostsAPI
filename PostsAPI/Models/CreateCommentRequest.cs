using System.ComponentModel.DataAnnotations;

namespace PostsAPI.Models
{
    public class CreateCommentRequest
    {
        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public string Content { get; set; }
    }
}
