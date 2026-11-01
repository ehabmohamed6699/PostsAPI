using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PostsAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PostsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AppDbContext context, IConfiguration config) : ControllerBase
    {
        // instance of hashing object to hash and check passwords
        private readonly PasswordHasher<User> _hasher = new();
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest user)
        {
            // check if user with same email already exists (index is advantageous here)
            var userExists = await context.Users.AnyAsync(u => u.Email == user.Email);
            if (userExists) return Conflict();
            var newUser = new User { Email = user.Email };
            // hash the sent password and store it in the new user's object
            newUser.PasswordHash = _hasher.HashPassword(newUser, user.Password);
            context.Users.Add(newUser);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest user)
        {
            // check first the email
            var existingUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser == null) return Unauthorized();
            // verifying password with the hasher (hashing here is advantageous)
            var result = _hasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, user.Password);
            if (result == PasswordVerificationResult.Failed) return Unauthorized();
            // return the access token for the user
            return Ok(new { token = CreateToken(existingUser) });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(claims);
        }
        private string CreateToken(User user)
        {
            // ?
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // I guess here is what the client should keep (like for example email for displaying user's email for him and id for sent requests later which the server will extract)
            var claims = new List<Claim> { 
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (ClaimTypes.Role, user.Role)
            };

            // ?
            var token = new JwtSecurityToken(
                issuer: config["JwtSettings:Issuer"],
                audience: config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(config["JwtSettings:ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
