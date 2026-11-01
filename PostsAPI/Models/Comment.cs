using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostsAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public int PostId { get; set; }
        public int? UserId { get; set; }

        public virtual Post Post { get; set; }
        public User? User { get; set; }
    }
}
