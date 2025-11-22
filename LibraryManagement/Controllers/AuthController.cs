using LibraryManagement.Context;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using LibraryManagement.Service.interfaces;
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
        private readonly IAuthService authService;

        public AuthController(LibraryContext context, ITokenService tokenService, IAuthService authService)
        {
            this.context = context;
            this.tokenService = tokenService;
            this.authService = authService;
        }

        /// <summary>
        /// Helps to login user and returns back JWT token which can be used for further protected endpoints.         
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns>JWT token</returns>        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = await authService.Login(loginDto);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException unauthorizedException)
            {
                return Unauthorized(unauthorizedException.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");                
            }

        }

        /// <summary>
        /// Method is being used for registering the users
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns>Whether user has registered.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                await authService.Register(registerDto);
                return Ok(new { message = "User registered successfully" });
            }
            catch (BadHttpRequestException badRequest)
            {
                return BadRequest(badRequest.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");                
            }
        }
    }
}
