using LibraryManagement.Context;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using LibraryManagement.Utils;

namespace LibraryManagement.Service
{
    /// <summary>
    /// Implementation for Authentication related services
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly LibraryContext context;
        private readonly ITokenService tokenService;
        private readonly ILogger<AuthService> logger;

        public AuthService(LibraryContext context, ITokenService tokenService, ILogger<AuthService> logger)
        {
            this.context = context;
            this.tokenService = tokenService;
            this.logger = logger;
        }
        /// <summary>
        /// Method helps in logging in the user and return back JWT token.
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> Login(LoginDto loginDto)
        {
            try
            {
                var user = context.Users.FirstOrDefault(u =>
                u.Username == loginDto.Username && u.Password == loginDto.Password);

                if (user == null)
                    throw new UnauthorizedAccessException("Invalid credentials.");

                var token = tokenService.GenerateToken(user);
                return await Task.FromResult(token.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError("Error while logging in. The exception is {}", ex);
                throw ;
            }
        }

        /// <summary>
        /// Used for registering user against the system
        /// </summary>
        /// <param name="registerDto"></param>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Register(RegisterDto registerDto)
        {
            try
            {
                if (context.Users.Any(u => u.Username == registerDto.Username))
                    throw new BadHttpRequestException("Username already exists");

                var user = new User
                {
                    Username = registerDto.Username,
                    Password = registerDto.Password,
                    Role = Constants.CLIENT_ROLE
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error while registering new user. The exception is {}", ex);
                throw;
            }
        }
    }
}
