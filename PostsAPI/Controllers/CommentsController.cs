using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostsAPI.Models;
using System.Security.Claims;

namespace PostsAPI.Controllers
{
    [Route("api/posts/{postId:int}/[controller]")]
    [ApiController]
    public class CommentsController(AppDbContext context) : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(int postId, [FromBody] CreateCommentRequest comment)
        {
            var post = await context.Posts.AnyAsync(p => p.Id == postId);
            if (!post) return NotFound();
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Comment newComment = new Comment { Content = comment.Content, PostId = postId, UserId = int.Parse(sub!) };
            context.Comments.Add(newComment);
            await context.SaveChangesAsync();
            return Ok(newComment);
        }
    }
}
