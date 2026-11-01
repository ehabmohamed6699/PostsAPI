using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PostsAPI.Models
{
    public class AppDbContext(DbContextOptions<AppDbContext> opts): DbContext(opts)
    {

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();
            // created user index on email to ease search
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        // add Users collection to the context to manipulate users table
        public DbSet<User> Users { get; set; }
    }
}
