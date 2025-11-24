using LibraryManagement.Context;
using LibraryManagement.Controllers;
using LibraryManagement.Models.Dto;
using LibraryManagement.Service.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryManagement.Tests.Controllers
{
    public class AuthControllerTests
    {        
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {            
            _mockTokenService = new Mock<ITokenService>();
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(                
                _mockTokenService.Object,
                _mockAuthService.Object);
        }

        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "testpass"
            };
            var expectedToken = "mock.jwt.token";

            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginDto>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var response = okResult.Value;
            var tokenProperty = response!.GetType().GetProperty("token");
            Assert.NotNull(tokenProperty);
            Assert.Equal(expectedToken, tokenProperty.GetValue(response));

            _mockAuthService.Verify(s => s.Login(It.Is<LoginDto>(dto =>
                dto.Username == "testuser" && dto.Password == "testpass")), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "wrongpass"
            };
            var exceptionMessage = "Invalid credentials.";

            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginDto>()))
                .ThrowsAsync(new UnauthorizedAccessException(exceptionMessage));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Equal(exceptionMessage, unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_WhenServiceThrowsUnauthorizedException_ReturnsUnauthorizedWithMessage()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "user",
                Password = "pass"
            };
            var customMessage = "Account is locked";

            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginDto>()))
                .ThrowsAsync(new UnauthorizedAccessException(customMessage));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(customMessage, unauthorizedResult.Value);
        }


        [Theory]
        [InlineData("user1", "pass1")]
        [InlineData("admin@email.com", "Admin123!")]
        [InlineData("test_user", "P@ssw0rd")]
        public async Task Login_WithVariousValidInputs_CallsServiceCorrectly(
            string username, string password)
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = username,
                Password = password
            };
            var token = "valid.token";

            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginDto>()))
                .ReturnsAsync(token);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockAuthService.Verify(s => s.Login(It.Is<LoginDto>(dto =>
                dto.Username == username && dto.Password == password)), Times.Once);
        }


        #endregion

        #region Register Tests

        [Fact]
        public async Task Register_WithValidData_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Password = "newpass"
            };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var response = okResult.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("User registered successfully", messageProperty.GetValue(response));

            _mockAuthService.Verify(s => s.Register(It.Is<RegisterDto>(dto =>
                dto.Username == "newuser" && dto.Password == "newpass")), Times.Once);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Password = "password"
            };
            var exceptionMessage = "Username already exists";

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterDto>()))
                .ThrowsAsync(new BadHttpRequestException(exceptionMessage));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(exceptionMessage, badRequestResult.Value);
        }


        [Fact]
        public async Task Register_WhenServiceThrowsInvalidOperationException_Returns500()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Password = "newpass"
            };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterDto>()))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion
    }
}