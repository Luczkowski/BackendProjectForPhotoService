using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PTBackend2024.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PTBackend2024.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class authController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public authController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User newUser)
        {
            if (_context.Users.Any(u => u.Username == newUser.Username))
            {
                return BadRequest("Username already taken");
            }

            if (_context.Users.Any(u => u.Email == newUser.Email))
            {
                return BadRequest("Email is already in use");
            }

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Created();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto userLogin)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userLogin.Username && u.Password == userLogin.Password);
            if (user == null)
            {
                return Unauthorized("Wrong login or password");
            }

            var tokenString = GenerateJwtToken(user);
            return Ok(new {Token = tokenString});
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[] 
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
