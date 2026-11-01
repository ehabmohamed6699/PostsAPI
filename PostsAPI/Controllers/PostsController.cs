using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostsAPI.Models;
using PostsAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PostsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController(IPostService service) : ControllerBase
    {
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private IActionResult ToProblem(Error e) => e.Type switch { 
            ErrorType.Forbidden => Problem(statusCode: 403, title: e.Message),
            ErrorType.NotFound => Problem(statusCode: 404, title: e.Message),
            _ => Problem()
        };
        private IActionResult ToActionResult<T>(Result<T?> result) => result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
        private IActionResult ToActionResult(Result result) => result.IsSuccess ? NoContent() : ToProblem(result.Error!);
        [HttpGet]
        public async Task<IActionResult> GetAll(string? search = null, string? sort = null, int page = 1, int pageSize = 5)
        {
            return ToActionResult<GetAllPostsResponse>((await service.GetPagedAsync(search, sort, page, pageSize))!);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPost(int id)
        {
            return ToActionResult<PostResponse?>(await service.GetPostByIdAsync(id));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreatePostRequest post)
        {
            var result = await service.CreateAsync(post, CurrentUserId);
            return CreatedAtAction(nameof(GetPost), new { id = result.Value!.Id}, result.Value);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePostRequest post)
        {
            var result = ToActionResult(await service.UpdateAsync(id, CurrentUserId, post));
            return result;
        }

        [HttpPut("{id:int}/tags/{tagId:int}")]
        [Authorize]
        public async Task<IActionResult> AttachTag(int id, int tagId)
        {   
            var result = ToActionResult(await service.AttachTagAsync(id, CurrentUserId, tagId));
            return result;
        }

        [HttpPost("{id:int}/like")]
        [Authorize]
        public async Task<IActionResult> Like(int id)
        {
            var result = ToActionResult(await service.LikeAsync(id, CurrentUserId));
            return result;
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var result = ToActionResult(await service.DeleteAsync(id, CurrentUserId));
            return result;
        }
    }
}
