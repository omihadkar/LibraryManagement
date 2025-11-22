using LibraryManagement.Models;
using LibraryManagement.Service;
using LibraryManagement.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace LibraryManagement.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;
        private readonly string _jwtKey = "ThisIsAVerySecureKeyForTestingPurposesOnly12345";
        private readonly string _jwtIssuer = "TestIssuer";
        private readonly string _jwtAudience = "TestAudience";
        private readonly string _jwtExpiry = "60";

        public TokenServiceTests()
        {
            // Setup configuration
            var inMemorySettings = new Dictionary<string, string>
            {
                { Constants.JWT_KEY, _jwtKey },
                { Constants.JWT_ISSUER, _jwtIssuer },
                { Constants.JWT_AUDIENCE, _jwtAudience },
                { Constants.JWT_EXPIRY, _jwtExpiry }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _tokenService = new TokenService(_configuration);
        }

        [Fact]
        public void GenerateToken_ValidUser_ReturnsValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "User"
            };

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.True(token.Split('.').Length == 3); // JWT has 3 parts: header.payload.signature
        }

        [Fact]
        public void GenerateToken_ValidUser_TokenContainsCorrectClaims()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "User"
            };

            // Act
            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            Assert.Equal(user.Username, jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal(user.Role, jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            Assert.Equal(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
        }

      
        [Fact]
        public void GenerateToken_ValidUser_TokenHasCorrectExpiration()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "User"
            };
            var expectedExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtExpiry));

            // Act
            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            Assert.NotNull(jwtToken.ValidTo);
            // Allow 1 minute tolerance for test execution time
            Assert.True(Math.Abs((jwtToken.ValidTo - expectedExpiry).TotalMinutes) < 1);
        }

        [Fact]
        public void GenerateToken_LibrarianUser_TokenContainsLibrarianRole()
        {
            // Arrange
            var user = new User
            {
                Id = 2,
                Username = "librarian",
                Role = Constants.LIBRARIAN_ROLE
            };

            // Act
            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            var roleClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value;
            Assert.Equal(Constants.LIBRARIAN_ROLE, roleClaim);
        }

        [Fact]
        public void GenerateToken_ValidUser_TokenCanBeValidated()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "User"
            };

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Act
            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();

            // Assert - Should not throw exception
            var principal = handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
        }
    }
}