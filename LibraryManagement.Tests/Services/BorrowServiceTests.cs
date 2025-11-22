using LibraryManagement.Context;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Service;
using LibraryManagement.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LibraryManagement.Tests.Services
{
    public class BorrowServiceTests : IDisposable
    {
        private readonly LibraryContext _context;
        private readonly Mock<ILogger<BorrowService>> _mockLogger;
        private readonly BorrowService _borrowService;

        public BorrowServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LibraryContext(options);
            _mockLogger = new Mock<ILogger<BorrowService>>();
            _borrowService = new BorrowService(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region BorrowBook Tests

        [Fact]
        public async Task BorrowBook_ValidRequest_Success()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Test Book", Author = "Author", ISBN = "123", AvailableCopies = 5, TotalCopies = 5 };
            var user = new User { Id = 1, Username = "testuser", Role = "Client" };

            _context.Books.Add(book);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            await _borrowService.BorrowBook(1, 1);

            // Assert
            var updatedBook = await _context.Books.FindAsync(1);
            var borrowRecord = await _context.BorrowRecords.FirstOrDefaultAsync(br => br.BookId == 1 && br.UserId == 1);

            Assert.NotNull(borrowRecord);
            Assert.Equal(4, updatedBook.AvailableCopies);
            Assert.False(borrowRecord.IsReturned);
            Assert.Equal(DateTime.UtcNow.Date, borrowRecord.BorrowDate.Date);
        }

        [Fact]
        public async Task BorrowBook_BookNotFound_ThrowsNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _borrowService.BorrowBook(999, 1));
        }

        [Fact]
        public async Task BorrowBook_NoCopiesAvailable_ThrowsBadHttpRequestException()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Test Book", Author = "Author", ISBN = "123", AvailableCopies = 0, TotalCopies = 5 };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(() => _borrowService.BorrowBook(1, 1));
            Assert.Equal("No copies available", exception.Message);
        }

        #endregion

        #region GetAllBorrows Tests

        [Fact]
        public async Task GetAllBorrows_ReturnsAllBorrowRecords()
        {
            // Arrange
            var book1 = new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "111", AvailableCopies = 5, TotalCopies = 5 };
            var book2 = new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "222", AvailableCopies = 3, TotalCopies = 5 };
            var user1 = new User { Id = 1, Username = "user1", Role = "Client" };
            var user2 = new User { Id = 2, Username = "user2", Role = "Client" };

            _context.Books.AddRange(book1, book2);
            _context.Users.AddRange(user1, user2);
            _context.BorrowRecords.AddRange(
                new BorrowRecord { Id = 1, UserId = 1, BookId = 1, BorrowDate = DateTime.UtcNow, IsReturned = false },
                new BorrowRecord { Id = 2, UserId = 2, BookId = 2, BorrowDate = DateTime.UtcNow, IsReturned = true, ReturnDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _borrowService.GetAllBorrows();

            // Assert
            var borrows = result.ToList();
            Assert.Equal(2, borrows.Count);
        }

        [Fact]
        public async Task GetAllBorrows_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _borrowService.GetAllBorrows();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetMyBorrows Tests

        [Fact]
        public async Task GetMyBorrows_ReturnsUserSpecificBorrows()
        {
            // Arrange
            var book1 = new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "111", AvailableCopies = 5, TotalCopies = 5 };
            var book2 = new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "222", AvailableCopies = 3, TotalCopies = 5 };
            var user1 = new User { Id = 1, Username = "user1", Role = "Client" };
            var user2 = new User { Id = 2, Username = "user2", Role = "Client" };

            _context.Books.AddRange(book1, book2);
            _context.Users.AddRange(user1, user2);
            _context.BorrowRecords.AddRange(
                new BorrowRecord { Id = 1, UserId = 1, BookId = 1, BorrowDate = DateTime.UtcNow, IsReturned = false },
                new BorrowRecord { Id = 2, UserId = 2, BookId = 2, BorrowDate = DateTime.UtcNow, IsReturned = false },
                new BorrowRecord { Id = 3, UserId = 1, BookId = 2, BorrowDate = DateTime.UtcNow, IsReturned = true, ReturnDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _borrowService.GetMyBorrows(1);

            // Assert
            var borrows = result.ToList();
            Assert.Equal(2, borrows.Count);
        }


        #endregion

        #region ReturnBook Tests

        [Fact]
        public async Task ReturnBook_ValidRequest_Success()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Test Book", Author = "Author", ISBN = "123", AvailableCopies = 4, TotalCopies = 5 };
            var user = new User { Id = 1, Username = "testuser", Role = "Client" };
            var borrowRecord = new BorrowRecord { Id = 1, UserId = 1, BookId = 1, BorrowDate = DateTime.UtcNow.AddDays(-5), IsReturned = false };

            _context.Books.Add(book);
            _context.Users.Add(user);
            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            var claimsPrincipal = CreateClaimsPrincipal(1, "testuser", "User");

            // Act
            await _borrowService.ReturnBook(1, claimsPrincipal);

            // Assert
            var updatedBorrow = await _context.BorrowRecords.FindAsync(1);
            var updatedBook = await _context.Books.FindAsync(1);

            Assert.True(updatedBorrow.IsReturned);
            Assert.NotNull(updatedBorrow.ReturnDate);
            Assert.Equal(5, updatedBook.AvailableCopies);
        }

        [Fact]
        public async Task ReturnBook_BorrowRecordNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimsPrincipal = CreateClaimsPrincipal(1, "testuser", "User");

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _borrowService.ReturnBook(999, claimsPrincipal));
        }


        #endregion

        #region Helper Methods

        private ClaimsPrincipal CreateClaimsPrincipal(int userId, string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            return new ClaimsPrincipal(identity);
        }

        #endregion
    }
}