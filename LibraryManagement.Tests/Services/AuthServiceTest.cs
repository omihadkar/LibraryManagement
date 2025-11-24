using LibraryManagement.Context;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using LibraryManagement.Service;
using LibraryManagement.Service.interfaces;
using LibraryManagement.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LibraryManagement.Tests.Service
{
    public class AuthServiceTests : IDisposable
    {
        private readonly LibraryContext _context;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new LibraryContext(options);

            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_context, _mockTokenService.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "testpass",
                Role = Constants.CLIENT_ROLE
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "testpass"
            };

            var expectedToken = "mock.jwt.token";
            _mockTokenService.Setup(s => s.GenerateToken(It.IsAny<User>()))
                .Returns(expectedToken);

            // Act
            var result = await _authService.Login(loginDto);

            // Assert
            Assert.Equal(expectedToken, result);
            _mockTokenService.Verify(s => s.GenerateToken(It.Is<User>(u =>
                u.Username == "testuser" && u.Password == "testpass")), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidUsername_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "testpass",
                Role = Constants.CLIENT_ROLE
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Username = "wronguser",
                Password = "testpass"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.Login(loginDto));
            Assert.Equal("Invalid credentials.", exception.Message);
            _mockTokenService.Verify(s => s.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = "testpass",
                Role = Constants.CLIENT_ROLE
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "wrongpass"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.Login(loginDto));
            Assert.Equal("Invalid credentials.", exception.Message);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "nonexistent",
                Password = "password"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.Login(loginDto));
            Assert.Equal("Invalid credentials.", exception.Message);
        }
        
        #endregion

        #region Register Tests

        [Fact]
        public async Task Register_WithNewUsername_CreatesUser()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Password = "newpass"
            };

            // Act
            await _authService.Register(registerDto);

            // Assert
            var user = _context.Users.FirstOrDefault(u => u.Username == "newuser");
            Assert.NotNull(user);
            Assert.Equal("newuser", user.Username);
            Assert.Equal("newpass", user.Password);
            Assert.Equal(Constants.CLIENT_ROLE, user.Role);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ThrowsBadHttpRequestException()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                Username = "existinguser",
                Password = "password",
                Role = Constants.CLIENT_ROLE
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Password = "newpass"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(
                () => _authService.Register(registerDto));
            Assert.Equal("Username already exists", exception.Message);
        }

        [Fact]
        public async Task Register_WithExistingUsername_DoesNotCreateDuplicateUser()
        {
            // Arrange
            var existingUser = new User
            {
                Id = 1,
                Username = "existinguser",
                Password = "password",
                Role = Constants.CLIENT_ROLE
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Password = "newpass"
            };

            // Act
            try
            {
                await _authService.Register(registerDto);
            }
            catch (BadHttpRequestException)
            {
                // Expected exception
            }

            // Assert
            var userCount = _context.Users.Count(u => u.Username == "existinguser");
            Assert.Equal(1, userCount);
        }

        #endregion
    }
}