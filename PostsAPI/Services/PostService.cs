using Microsoft.EntityFrameworkCore;
using PostsAPI.Models;

namespace PostsAPI.Services;

public interface IPostService
{
    Task<Result<GetAllPostsResponse>> GetPagedAsync(string? search = null, string? sort = null, int page = 1, int pageSize = 5);
    Task<Result<PostResponse>> GetPostByIdAsync(int postId);
    Task<Result<PostResponse>> CreateAsync(CreatePostRequest post, int userId);
    Task<Result> LikeAsync(int postId, int userId);
    Task<Result> UpdateAsync(int postId, int userId, UpdatePostRequest request);
    Task<Result> DeleteAsync(int postId, int userId);
    Task<Result> AttachTagAsync(int postId, int userId, int tagId);
}
public class PostService(AppDbContext context) : IPostService
{

    public async Task<Result> AttachTagAsync(int postId, int userId, int tagId)
    {
        var found = await GetOwnedPostAsync(postId, userId, true);
        if (!found.IsSuccess) return new Result(found.Error);
        var post = found.Value!;
        if (post.Tags.Any(t => t.Id == tagId)) return new Result();
        var tag = await GetTagById(tagId);
        if (tag is null) return new Result(new Error(ErrorType.NotFound, $"Tag {tagId} was not found."));
        post.Tags.Add(tag);
        await context.SaveChangesAsync();
        return new Result();
    }

    public async Task<Result> DeleteAsync(int postId, int userId)
    {
        var found = await GetOwnedPostAsync(postId, userId);
        if(!found.IsSuccess) return new Result(found.Error);
        var postToDelete = found.Value!;
        context.Posts.Remove(postToDelete);
        await context.SaveChangesAsync();
        return new Result();
    }

    public async Task<Result> UpdateAsync(int postId, int userId, UpdatePostRequest request)
    {
        var found = await GetOwnedPostAsync(postId, userId);
        if(!found.IsSuccess) return new Result(found.Error);
        var postToUpdate = found.Value!;
        postToUpdate.Title = request.Title;
        postToUpdate.Content = request.Content;
        await context.SaveChangesAsync();
        return new Result();
    }
    public async Task<Result<GetAllPostsResponse>> GetPagedAsync(string? search = null, string? sort = null, int page = 1, int pageSize = 5)
    {
        page = Math.Clamp(page, 1, 100_000);
        pageSize = Math.Clamp(pageSize, 1, 20);
        var query = context.Posts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Content.Contains(search) || p.Title.Contains(search));
        query = sort switch
        {
            "likes" => query.OrderByDescending(p => p.Likes).ThenBy(p => p.Id),
            "oldest" => query.OrderBy(p => p.Id),
            _ => query.OrderByDescending(p => p.Id)
        };
        int totalRecords = await query.CountAsync();
        int lastPage = totalRecords / pageSize;
        if (totalRecords % pageSize != 0) lastPage++;
        query = query.Skip((page - 1) * pageSize).Take(pageSize);
        var posts = await query.Include(p => p.Tags).Include(p => p.Comments).AsSplitQuery().Select(p => new PostResponse(p)).ToListAsync();
        return new(Value: new GetAllPostsResponse(posts, totalRecords, lastPage), null);
    }

    public async Task<Result<PostResponse>> GetPostByIdAsync(int postId)
    {
        var post = await context.Posts.AsNoTracking().Include(p => p.Tags).Include(p => p.Author).Include(p => p.Comments).AsSplitQuery().FirstOrDefaultAsync(p => p.Id == postId);
        return post == null ? new Result<PostResponse>(null, new Error(ErrorType.NotFound, $"Post {postId} was not found.")) : new Result<PostResponse>(new PostResponse(post));
    }
    public async Task<Result<PostResponse>> CreateAsync(CreatePostRequest post, int userId)
    {
        Post newPost = new Post { Title = post.Title, Content = post.Content, AuthorId = userId };
        context.Posts.Add(newPost);
        await context.SaveChangesAsync();
        return new(Value: new PostResponse(newPost), Error: null);
    }
    public async Task<Result> LikeAsync(int postId, int userId)
    {
        var rows = await context.Posts.Where(p => p.Id == postId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Likes, p => p.Likes + 1));
        return rows == 0 ? new(new Error(ErrorType.NotFound, $"Post {postId} was not found.")) : new();
    }
    private async Task<Tag?> GetTagById(int tagId) => (await context.Tags.FindAsync(tagId));
    private async Task<Result<Post>> GetOwnedPostAsync(int postId, int userId, bool includeTags = false)
    { 
        var post = (includeTags ? await context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == postId) : await context.Posts.FindAsync(postId));
        if (post is null) return new Result<Post>(null, new Error(ErrorType.NotFound, $"Post {postId} was not found."));
        if (post.AuthorId != userId) return new Result<Post>(null, new Error(ErrorType.Forbidden, "Only post owner can change this post."));
        return new Result<Post>(post);
    }

}
