using System.ComponentModel.DataAnnotations;

namespace PostsAPI.Models
{
    public class CreateTagRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
