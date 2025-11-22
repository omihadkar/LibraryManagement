using LibraryManagement.Context;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Service.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// Controller addreses activity around borrowing books.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BorrowController : ControllerBase
    {
        private readonly LibraryContext _context;
        private readonly IBorrowService borrowService;

        public BorrowController(LibraryContext context, IBorrowService borrowService)
        {
            _context = context;
            this.borrowService = borrowService;
        }

        /// <summary>
        /// Endopint for Borrowing book.
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Librarian,Client")]
        [HttpPost("borrow/{bookId}")]        
        public async Task<ActionResult> BorrowBook(int bookId, int userId)
        {
            /* var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
             await _context.SaveChangesAsync();*/
            try
            {
                await borrowService.BorrowBook(bookId, userId);
                return Ok(new { message = "Book borrowed successfully"});
            }
            catch (BadHttpRequestException badHttpException)
            {
                return BadRequest(badHttpException.Message);
            }
            catch (NotFoundException notFoundException)
            {
                return NotFound(notFoundException.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }

            
        }

        /// <summary>
        /// Endpoint for returning the book.
        /// </summary>
        /// <param name="borrowId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Librarian,Client")]
        [HttpPost("return/{borrowId}")]        
        public async Task<ActionResult> ReturnBook(int borrowId, int userId)
        {

            /* var borrowRecord = await _context.BorrowRecords
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

             await _context.SaveChangesAsync();*/

            try
            {
                await borrowService.ReturnBook(borrowId, userId);
                return Ok(new { message = "Book returned successfully" });
            }
            catch (NotFoundException notFoundException)
            {
                return NotFound(notFoundException.Message);
                
            }
            catch (BadHttpRequestException badHttpException)
            {
                return BadRequest(badHttpException.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
                throw;
            }
        }

        /// <summary>
        /// Get borrowed book record by user id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Librarian,Client")]
        [HttpGet("my-borrows/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyBorrows(int userId)
        {
            try
            {
                var borrows = await borrowService.GetMyBorrows(userId);
                return Ok(borrows);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Gets the record of all users who has borrowed the books
        /// </summary>
        /// <returns>Records of borrow</returns>
        [Authorize(Roles = "Librarian")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllBorrows()
        {
            try
            {
                var borrows = await borrowService.GetAllBorrows();
                return Ok(borrows);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
