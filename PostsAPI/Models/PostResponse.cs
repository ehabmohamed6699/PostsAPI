namespace PostsAPI.Models
{
    public class PostResponse
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public string Title { get; set; }

        public string Content { get; set; }

        public int Likes { get; set; }

        public string PostSlogan { get => $"[{Title}] {Content} ({Likes} likes)";}
        public UserResponse? Author { get; set; }
        public ICollection<CommentResponse> Comments { get; set; } = [];
        public ICollection<string> Tags { get; set; } = [];

        public PostResponse() { }

        public PostResponse(Post post)
        {
            this.Id = post.Id;
            this.AuthorId = post.AuthorId;
            this.Title = post.Title;
            this.Content = post.Content;
            this.Likes = post.Likes;
            this.Comments = post.Comments.Select(c => new CommentResponse { Id = c.Id, Content = c.Content}).ToList();
            this.Tags = post.Tags.Select(t => t.Name).ToList();
            this.Author = post.Author == null ? null : new UserResponse { Id = post.Author.Id, Email = post.Author.Email};
        }
    }

    public record GetAllPostsResponse(IEnumerable<PostResponse> Posts, int TotalPosts, int LastPage);
}
