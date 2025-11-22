using LibraryManagement.Context;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using LibraryManagement.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LibraryManagement.Tests.Services
{
    public class BookServiceTests : IDisposable
    {
        private readonly LibraryContext _context;
        private readonly Mock<ILogger<BookService>> _mockLogger;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LibraryContext(options);
            _mockLogger = new Mock<ILogger<BookService>>();
            _bookService = new BookService(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region CreateBook Tests

        [Fact]
        public async Task CreateBook_ValidBookDto_ReturnsCreatedBook()
        {
            // Arrange
            var bookDto = new BookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                Copies = 5
            };

            // Act
            var result = await _bookService.CreateBook(bookDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookDto.Title, result.Title);
            Assert.Equal(bookDto.Author, result.Author);
            Assert.Equal(bookDto.ISBN, result.ISBN);
            Assert.Equal(bookDto.Copies, result.TotalCopies);
            Assert.Equal(bookDto.Copies, result.AvailableCopies);
            Assert.True(result.Id > 0);
        }

     
        [Fact]
        public async Task CreateBook_AvailableCopiesEqualsTotal_WhenCreated()
        {
            // Arrange
            var bookDto = new BookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                Copies = 10
            };

            // Act
            var result = await _bookService.CreateBook(bookDto);

            // Assert
            Assert.Equal(result.TotalCopies, result.AvailableCopies);
        }

        [Fact]
        public async Task CreateBook_MultipleBooks_AllSavedSuccessfully()
        {
            // Arrange
            var bookDto1 = new BookDto { Title = "Book 1", Author = "Author 1", ISBN = "111", Copies = 5 };
            var bookDto2 = new BookDto { Title = "Book 2", Author = "Author 2", ISBN = "222", Copies = 3 };

            // Act
            var book1 = await _bookService.CreateBook(bookDto1);
            var book2 = await _bookService.CreateBook(bookDto2);

            // Assert
            var booksInDb = await _context.Books.ToListAsync();
            Assert.Equal(2, booksInDb.Count);
            Assert.Contains(booksInDb, b => b.Id == book1.Id);
            Assert.Contains(booksInDb, b => b.Id == book2.Id);
        }

        #endregion

        #region GetBooks Tests

        [Fact]
        public async Task GetBooks_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _bookService.GetBooks();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBooks_WithBooksInDatabase_ReturnsAllBooks()
        {
            // Arrange
            var books = new[]
            {
                new Book { Title = "Book 1", Author = "Author 1", ISBN = "111", TotalCopies = 5, AvailableCopies = 5 },
                new Book { Title = "Book 2", Author = "Author 2", ISBN = "222", TotalCopies = 3, AvailableCopies = 3 },
                new Book { Title = "Book 3", Author = "Author 3", ISBN = "333", TotalCopies = 7, AvailableCopies = 7 }
            };
            _context.Books.AddRange(books);
            await _context.SaveChangesAsync();

            // Act
            var result = await _bookService.GetBooks();

            // Assert
            var booksList = result.ToList();
            Assert.Equal(3, booksList.Count);
        }

        #endregion

        #region GetBook Tests

        [Fact]
        public async Task GetBook_ValidId_ReturnsBook()
        {
            // Arrange
            var book = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                TotalCopies = 5,
                AvailableCopies = 5
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Act
            var result = await _bookService.GetBook(book.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(book.Id, result.Id);
            Assert.Equal(book.Title, result.Title);
            Assert.Equal(book.Author, result.Author);
        }

        [Fact]
        public async Task GetBook_InvalidId_ThrowsNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _bookService.GetBook(999));
            Assert.Equal("Book Not found", exception.Message);
        }

        [Fact]
        public async Task GetBook_DeletedBook_ThrowsNotFoundException()
        {
            // Arrange
            var book = new Book { Title = "Test Book", Author = "Author", ISBN = "123", TotalCopies = 5, AvailableCopies = 5 };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            var bookId = book.Id;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _bookService.GetBook(bookId));
        }

        #endregion

        #region UpdateBook Tests

        [Fact]
        public async Task UpdateBook_ValidIdAndDto_UpdatesBookSuccessfully()
        {
            // Arrange
            var book = new Book
            {
                Title = "Original Title",
                Author = "Original Author",
                ISBN = "111",
                TotalCopies = 5,
                AvailableCopies = 5
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var updateDto = new BookDto
            {
                Title = "Updated Title",
                Author = "Updated Author",
                ISBN = "222",
                Copies = 5
            };

            // Act
            await _bookService.UpdateBook(book.Id, updateDto);

            // Assert
            var updatedBook = await _context.Books.FindAsync(book.Id);
            Assert.Equal("Updated Title", updatedBook.Title);
            Assert.Equal("Updated Author", updatedBook.Author);
            Assert.Equal("222", updatedBook.ISBN);
        }


        #endregion

        #region DeleteBook Tests

        [Fact]
        public async Task DeleteBook_ValidIdNoActiveBorrows_DeletesSuccessfully()
        {
            // Arrange
            var book = new Book
            {
                Title = "Test Book",
                Author = "Author",
                ISBN = "123",
                TotalCopies = 5,
                AvailableCopies = 5
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            var bookId = book.Id;

            // Act
            await _bookService.DeleteBook(bookId);

            // Assert
            var deletedBook = await _context.Books.FindAsync(bookId);
            Assert.Null(deletedBook);
        }

        #endregion
    }
}