using LibraryManagement.Controllers;
using LibraryManagement.Exceptions;
using LibraryManagement.Service.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LibraryManagement.Tests.Controllers
{
    public class BorrowControllerTests
    {
        private readonly Mock<IBorrowService> _mockBorrowService;
        private readonly BorrowController _controller;
        private readonly ClaimsPrincipal _testUser;

        public BorrowControllerTests()
        {
            _mockBorrowService = new Mock<IBorrowService>();
            _controller = new BorrowController(_mockBorrowService.Object);

            // Setup test user with claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "Client")
            };
            _testUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _testUser }
            };
        }

        #region BorrowBook Tests

        [Fact]
        public async Task BorrowBook_WithValidData_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var bookId = 1;
            var userId = 1;

            _mockBorrowService.Setup(s => s.BorrowBook(bookId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.BorrowBook(bookId, userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var response = okResult.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Book borrowed successfully", messageProperty.GetValue(response));

            _mockBorrowService.Verify(s => s.BorrowBook(bookId, userId), Times.Once);
        }

        [Fact]
        public async Task BorrowBook_WithInvalidBookId_ReturnsNotFound()
        {
            // Arrange
            var bookId = 999;
            var userId = 1;
            var exceptionMessage = "Book not found";

            _mockBorrowService.Setup(s => s.BorrowBook(bookId, userId))
                .ThrowsAsync(new NotFoundException(exceptionMessage));

            // Act
            var result = await _controller.BorrowBook(bookId, userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal(exceptionMessage, notFoundResult.Value);
        }

        [Fact]
        public async Task BorrowBook_WhenNoAvailableCopies_ReturnsBadRequest()
        {
            // Arrange
            var bookId = 1;
            var userId = 1;
            var exceptionMessage = "No available copies";

            _mockBorrowService.Setup(s => s.BorrowBook(bookId, userId))
                .ThrowsAsync(new BadHttpRequestException(exceptionMessage));

            // Act
            var result = await _controller.BorrowBook(bookId, userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(exceptionMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task BorrowBook_WhenUserAlreadyBorrowed_ReturnsBadRequest()
        {
            // Arrange
            var bookId = 1;
            var userId = 1;
            var exceptionMessage = "User has already borrowed this book";

            _mockBorrowService.Setup(s => s.BorrowBook(bookId, userId))
                .ThrowsAsync(new BadHttpRequestException(exceptionMessage));

            // Act
            var result = await _controller.BorrowBook(bookId, userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(exceptionMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task BorrowBook_WithInvalidUserId_ReturnsNotFound()
        {
            // Arrange
            var bookId = 1;
            var userId = 999;
            var exceptionMessage = "User not found";

            _mockBorrowService.Setup(s => s.BorrowBook(bookId, userId))
                .ThrowsAsync(new NotFoundException(exceptionMessage));

            // Act
            var result = await _controller.BorrowBook(bookId, userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(exceptionMessage, notFoundResult.Value);
        }

        #endregion

        #region ReturnBook Tests

        [Fact]
        public async Task ReturnBook_WithValidBorrowId_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var borrowId = 1;

            _mockBorrowService.Setup(s => s.ReturnBook(borrowId, It.IsAny<ClaimsPrincipal>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ReturnBook(borrowId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var response = okResult.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Book returned successfully", messageProperty.GetValue(response));

            _mockBorrowService.Verify(s => s.ReturnBook(borrowId, It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task ReturnBook_WithInvalidBorrowId_ReturnsNotFound()
        {
            // Arrange
            var borrowId = 999;
            var exceptionMessage = "Borrow record not found";

            _mockBorrowService.Setup(s => s.ReturnBook(borrowId, It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new NotFoundException(exceptionMessage));

            // Act
            var result = await _controller.ReturnBook(borrowId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal(exceptionMessage, notFoundResult.Value);
        }

        [Fact]
        public async Task ReturnBook_WhenAlreadyReturned_ReturnsBadRequest()
        {
            // Arrange
            var borrowId = 1;
            var exceptionMessage = "Book has already been returned";

            _mockBorrowService.Setup(s => s.ReturnBook(borrowId, It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new BadHttpRequestException(exceptionMessage));

            // Act
            var result = await _controller.ReturnBook(borrowId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(exceptionMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnBook_WhenUserNotAuthorized_ReturnsBadRequest()
        {
            // Arrange
            var borrowId = 1;
            var exceptionMessage = "User is not authorized to return this book";

            _mockBorrowService.Setup(s => s.ReturnBook(borrowId, It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new ForbiddenActionException(exceptionMessage));

            // Act
            var result = await _controller.ReturnBook(borrowId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(exceptionMessage, badRequestResult.Value);
        }

        #endregion

        #region GetMyBorrows Tests

        [Fact]
        public async Task GetMyBorrows_WithValidUserId_ReturnsOkWithBorrows()
        {
            // Arrange
            var userId = 1;
            var borrows = new List<object>
            {
                new { BorrowId = 1, BookTitle = "Book 1", BorrowDate = DateTime.Now },
                new { BorrowId = 2, BookTitle = "Book 2", BorrowDate = DateTime.Now }
            };

            _mockBorrowService.Setup(s => s.GetMyBorrows(userId))
                .ReturnsAsync(borrows);

            // Act
            var result = await _controller.GetMyBorrows(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            var returnedBorrows = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, returnedBorrows.Count());

            _mockBorrowService.Verify(s => s.GetMyBorrows(userId), Times.Once);
        }

        [Fact]
        public async Task GetMyBorrows_WithNoBorrows_ReturnsEmptyList()
        {
            // Arrange
            var userId = 1;

            _mockBorrowService.Setup(s => s.GetMyBorrows(userId))
                .ReturnsAsync(new List<object>());

            // Act
            var result = await _controller.GetMyBorrows(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBorrows = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Empty(returnedBorrows);
        }

        [Fact]
        public async Task GetMyBorrows_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            var userId = 1;

            _mockBorrowService.Setup(s => s.GetMyBorrows(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetMyBorrows(userId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }

        #endregion

        #region GetAllBorrows Tests

        [Fact]
        public async Task GetAllBorrows_ReturnsOkWithAllBorrows()
        {
            // Arrange
            var allBorrows = new List<object>
            {
                new { BorrowId = 1, UserId = 1, BookTitle = "Book 1", BorrowDate = DateTime.Now },
                new { BorrowId = 2, UserId = 2, BookTitle = "Book 2", BorrowDate = DateTime.Now },
                new { BorrowId = 3, UserId = 1, BookTitle = "Book 3", BorrowDate = DateTime.Now }
            };

            _mockBorrowService.Setup(s => s.GetAllBorrows())
                .ReturnsAsync(allBorrows);

            // Act
            var result = await _controller.GetAllBorrows();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            var returnedBorrows = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(3, returnedBorrows.Count());

            _mockBorrowService.Verify(s => s.GetAllBorrows(), Times.Once);
        }

        [Fact]
        public async Task GetAllBorrows_WithNoBorrows_ReturnsEmptyList()
        {
            // Arrange
            _mockBorrowService.Setup(s => s.GetAllBorrows())
                .ReturnsAsync(new List<object>());

            // Act
            var result = await _controller.GetAllBorrows();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBorrows = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Empty(returnedBorrows);
        }

        [Fact]
        public async Task GetAllBorrows_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            _mockBorrowService.Setup(s => s.GetAllBorrows())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllBorrows();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }
        #endregion
    }
}