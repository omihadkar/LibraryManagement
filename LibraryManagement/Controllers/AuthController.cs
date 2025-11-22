using LibraryManagement.Context;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using LibraryManagement.Service;
using LibraryManagement.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// Controller for managing Authnetication related actions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly LibraryContext context;
        private readonly ITokenService tokenService;

        public AuthController(LibraryContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        /// <summary>
        /// Helps to login user and returns back JWT token which can be used for further protected endpoints.         
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns>JWT token</returns>        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = context.Users.FirstOrDefault(u =>
                u.Username == loginDto.Username && u.Password == loginDto.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = tokenService.GenerateToken(user);
            return Ok(new { token });
        }

        /// <summary>
        /// Method is being used for registering the users
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns>Whether user has registered.</returns>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto registerDto)
        {
            if (context.Users.Any(u => u.Username == registerDto.Username))
                return BadRequest(new { message = "Username already exists" });

            var user = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password,
                Role = Constants.CLIENT_ROLE
            };

            context.Users.Add(user);
            context.SaveChanges();
            return Ok(new { message = "User registered successfully", userId = user.Id });
        }
    }
}
