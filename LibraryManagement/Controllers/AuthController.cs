using LibraryManagement.Context;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using LibraryManagement.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly LibraryContext context;
        //private readonly string _jwtKey = "19b4cbbfe1c17de8df5bb4c6c4078400";
        private readonly ITokenService tokenService; 

        public AuthController(LibraryContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = context.Users.FirstOrDefault(u =>
                u.Username == loginDto.Username && u.Password == loginDto.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = tokenService.GenerateToken(user);
            return Ok(new { token, role = user.Role, userId = user.Id });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto registerDto)
        {
            if (context.Users.Any(u => u.Username == registerDto.Username))
                return BadRequest(new { message = "Username already exists" });

            var user = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password,
                Role = "Client"
            };

            context.Users.Add(user);
            context.SaveChanges();

            return Ok(new { message = "User registered successfully", userId = user.Id });
        }

       /* private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }*/
    }
}
