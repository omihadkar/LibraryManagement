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
            try
            {
                await borrowService.BorrowBook(bookId, userId);
                return Ok(new { message = "Book borrowed successfully" });
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
        public async Task<ActionResult> ReturnBook(int borrowId)
        {
            try
            {
                await borrowService.ReturnBook(borrowId, User);
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
            catch (ForbiddenActionException forbiddenException)
            {
                return BadRequest(forbiddenException.Message);
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
