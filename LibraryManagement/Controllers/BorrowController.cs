using LibraryManagement.Context;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BorrowController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BorrowController(LibraryContext context)
        {
            _context = context;
        }

        [HttpPost("borrow/{bookId}")]
        public async Task<ActionResult> BorrowBook(int bookId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
                return NotFound(new { message = "Book not found" });

            if (book.AvailableCopies <= 0)
                return BadRequest(new { message = "No copies available" });

            var hasUnreturnedBook = await _context.BorrowRecords
                .AnyAsync(br => br.UserId == userId && br.BookId == bookId && !br.IsReturned);

            if (hasUnreturnedBook)
                return BadRequest(new { message = "You already have this book borrowed" });

            var borrowRecord = new BorrowRecord
            {
                UserId = userId,
                BookId = bookId,
                BorrowDate = DateTime.UtcNow,
                IsReturned = false
            };

            book.AvailableCopies--;
            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Book borrowed successfully", borrowId = borrowRecord.Id });
        }

        [HttpPost("return/{borrowId}")]
        public async Task<ActionResult> ReturnBook(int borrowId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var borrowRecord = await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == borrowId);

            if (borrowRecord == null)
                return NotFound(new { message = "Borrow record not found" });

            if (borrowRecord.UserId != userId && !User.IsInRole("Librarian"))
                return Forbid();

            if (borrowRecord.IsReturned)
                return BadRequest(new { message = "Book already returned" });

            borrowRecord.IsReturned = true;
            borrowRecord.ReturnDate = DateTime.UtcNow;
            borrowRecord.Book.AvailableCopies++;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Book returned successfully" });
        }

        [HttpGet("my-borrows")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyBorrows()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var borrows = await _context.BorrowRecords
                .Include(br => br.Book)
                .Where(br => br.UserId == userId)
                .Select(br => new
                {
                    br.Id,
                    br.BookId,
                    BookTitle = br.Book.Title,
                    BookAuthor = br.Book.Author,
                    br.BorrowDate,
                    br.ReturnDate,
                    br.IsReturned
                })
                .ToListAsync();

            return Ok(borrows);
        }

        [Authorize(Roles = "Librarian")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllBorrows()
        {
            var borrows = await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .Select(br => new
                {
                    br.Id,
                    br.UserId,
                    Username = br.User.Username,
                    br.BookId,
                    BookTitle = br.Book.Title,
                    br.BorrowDate,
                    br.ReturnDate,
                    br.IsReturned
                })
                .ToListAsync();

            return Ok(borrows);
        }
    }
}
