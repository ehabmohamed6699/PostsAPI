using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostsAPI.Models;

namespace PostsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await context.Tags.AsNoTracking().Select(t => new { Id = t.Id, Name = t.Name }).ToListAsync();
            return Ok(tags);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTag(int id)
        {
            var tag = await context.Tags.FindAsync(id);
            return tag == null ? NotFound() : Ok(new { Id = tag.Id, Name = tag.Name});
        }
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddTag([FromBody] CreateTagRequest tag)
        {
            var existingTag = await context.Tags.AnyAsync(t => t.Name == tag.Name);
            if (existingTag) return Problem(statusCode: 409, title:"Tag already exists!");
            Tag newTag = new Tag { Name = tag.Name };
            context.Tags.Add(newTag);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTag), new { Id = newTag.Id}, new { Id = newTag.Id, Name = newTag.Name });
        }
    }
}
