using System.ComponentModel.DataAnnotations;

namespace PostsAPI.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        public ICollection<Post> Posts { get; set; } = [];
    }
}
