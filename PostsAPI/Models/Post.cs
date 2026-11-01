using System.ComponentModel.DataAnnotations.Schema;

namespace PostsAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; } = 0;
        public int? AuthorId { get; set; }

        public User? Author { get; set; }
        public ICollection<Tag> Tags { get; set; } = [];
        public ICollection<Comment> Comments { get; set; } = [];

    }
}
