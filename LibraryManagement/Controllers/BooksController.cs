using LibraryManagement.Context;
using LibraryManagement.Models.Dto;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Exceptions;
using LibraryManagement.Service.interfaces;

namespace LibraryManagement.Controllers
{
    /// <summary>
    /// Controller contains different actions related to books
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService bookService;

        public BooksController(IBookService bookService)
        {
            this.bookService = bookService;
        }

        /// <summary>
        /// Fetches all the books.
        /// TODO: Pagination is pending
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            try
            {
                return new OkObjectResult(await bookService.GetBooks());
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
            
        }

        /// <summary>
        /// Get specific book based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            try
            {
                var book = await bookService.GetBook(id);
                return new OkObjectResult(book);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Create new book record. Restricted to Librarian.
        /// </summary>
        /// <param name="bookDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Librarian")]
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook([FromBody] BookDto bookDto)
        {
            try
            {
                var bookCreated = await bookService.CreateBook(bookDto);
                return CreatedAtAction(nameof(GetBook), new { id = bookCreated?.Id }, bookCreated);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Update book record. Restricted to Librarian.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Librarian")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookDto bookDto)
        {
            try
            {
                await bookService.UpdateBook(id, bookDto);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Delete book record. Restricted to Librarian.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Librarian")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                await bookService.DeleteBook(id);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BooksCanNotDeleteException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
