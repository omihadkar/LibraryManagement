using LibraryManagement.Controllers;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using LibraryManagement.Service.interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryManagement.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BooksController(_mockBookService.Object);
        }

        #region GetBooks Tests

        [Fact]
        public async Task GetBooks_ReturnsOkWithListOfBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "123", AvailableCopies = 5 },
                new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "456", AvailableCopies = 3 }
            };

            _mockBookService.Setup(s => s.GetBooks())
                .ReturnsAsync(books);

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooks = Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
            Assert.Equal(2, returnedBooks.Count());
            _mockBookService.Verify(s => s.GetBooks(), Times.Once);
        }

        [Fact]
        public async Task GetBooks_ReturnsEmptyList_WhenNoBooksExist()
        {
            // Arrange
            _mockBookService.Setup(s => s.GetBooks())
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooks = Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
            Assert.Empty(returnedBooks);
        }

        [Fact]
        public async Task GetBooks_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            _mockBookService.Setup(s => s.GetBooks())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }

        #endregion

        #region GetBook Tests

        [Fact]
        public async Task GetBook_WithValidId_ReturnsOkWithBook()
        {
            // Arrange
            var bookId = 1;
            var book = new Book
            {
                Id = bookId,
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "123456",
                AvailableCopies = 5
            };

            _mockBookService.Setup(s => s.GetBook(bookId))
                .ReturnsAsync(book);

            // Act
            var result = await _controller.GetBook(bookId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBook = Assert.IsType<Book>(okResult.Value);
            Assert.Equal(bookId, returnedBook.Id);
            Assert.Equal("Test Book", returnedBook.Title);
            _mockBookService.Verify(s => s.GetBook(bookId), Times.Once);
        }

        [Fact]
        public async Task GetBook_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var bookId = 999;
            _mockBookService.Setup(s => s.GetBook(bookId))
                .ThrowsAsync(new NotFoundException("Book not found"));

            // Act
            var result = await _controller.GetBook(bookId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetBook_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            var bookId = 1;
            _mockBookService.Setup(s => s.GetBook(bookId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetBook(bookId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }


        #endregion

        #region CreateBook Tests

        [Fact]
        public async Task CreateBook_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var bookDto = new BookDto
            {
                Title = "New Book",
                Author = "New Author",
                ISBN = "789",
                Copies = 10
            };

            var createdBook = new Book
            {
                Id = 1,
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                AvailableCopies = bookDto.Copies
            };

            _mockBookService.Setup(s => s.CreateBook(It.IsAny<BookDto>()))
                .ReturnsAsync(createdBook);

            // Act
            var result = await _controller.CreateBook(bookDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetBook), createdAtActionResult.ActionName);
            Assert.Equal(createdBook.Id, ((Book)createdAtActionResult.Value!).Id);

            var routeValues = createdAtActionResult.RouteValues;
            Assert.NotNull(routeValues);
            Assert.Equal(createdBook.Id, routeValues["id"]);

            _mockBookService.Verify(s => s.CreateBook(It.Is<BookDto>(dto =>
                dto.Title == bookDto.Title &&
                dto.Author == bookDto.Author &&
                dto.ISBN == bookDto.ISBN)), Times.Once);
        }

      

        [Fact]
        public async Task CreateBook_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            var bookDto = new BookDto
            {
                Title = "New Book",
                Author = "New Author",
                ISBN = "789",
                Copies = 10
            };

            _mockBookService.Setup(s => s.CreateBook(It.IsAny<BookDto>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateBook(bookDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }

        #endregion

        #region UpdateBook Tests

        [Fact]
        public async Task UpdateBook_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var bookId = 1;
            var bookDto = new BookDto
            {
                Title = "Updated Book",
                Author = "Updated Author",
                ISBN = "999",
                Copies = 15
            };

            _mockBookService.Setup(s => s.UpdateBook(bookId, It.IsAny<BookDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateBook(bookId, bookDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockBookService.Verify(s => s.UpdateBook(bookId, It.Is<BookDto>(dto =>
                dto.Title == bookDto.Title &&
                dto.Author == bookDto.Author &&
                dto.ISBN == bookDto.ISBN)), Times.Once);
        }

        [Fact]
        public async Task UpdateBook_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var bookId = 999;
            var bookDto = new BookDto
            {
                Title = "Updated Book",
                Author = "Updated Author",
                ISBN = "999",
                Copies = 15
            };

            _mockBookService.Setup(s => s.UpdateBook(bookId, It.IsAny<BookDto>()))
                .ThrowsAsync(new NotFoundException("Book not found"));

            // Act
            var result = await _controller.UpdateBook(bookId, bookDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateBook_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            var bookId = 1;
            var bookDto = new BookDto
            {
                Title = "Updated Book",
                Author = "Updated Author",
                ISBN = "999",
                Copies = 15
            };

            _mockBookService.Setup(s => s.UpdateBook(bookId, It.IsAny<BookDto>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateBook(bookId, bookDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred.", statusCodeResult.Value);
        }
  
        #endregion

        #region DeleteBook Tests

        [Fact]
        public async Task DeleteBook_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var bookId = 1;

            _mockBookService.Setup(s => s.DeleteBook(bookId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteBook(bookId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockBookService.Verify(s => s.DeleteBook(bookId), Times.Once);
        }

        [Fact]
        public async Task DeleteBook_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var bookId = 999;

            _mockBookService.Setup(s => s.DeleteBook(bookId))
                .ThrowsAsync(new NotFoundException("Book not found"));

            // Act
            var result = await _controller.DeleteBook(bookId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteBook_WhenBookHasBorrowedCopies_ReturnsBadRequest()
        {
            // Arrange
            var bookId = 1;
            var exceptionMessage = "Cannot delete book with active borrows";

            _mockBookService.Setup(s => s.DeleteBook(bookId))
                .ThrowsAsync(new BooksCanNotDeleteException(exceptionMessage));

            // Act
            var result = await _controller.DeleteBook(bookId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal(exceptionMessage, messageProperty.GetValue(response));
        }
        #endregion
    }
}