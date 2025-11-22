using LibraryManagement.Context;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Service
{
    /// <summary>
    /// Class for handling book related actions such as adding, getting, updating books.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly LibraryContext _context;
        private readonly ILogger<BookService> _logger;

        public BookService(LibraryContext context, ILogger<BookService> logger)
        {
            this._context = context;
            _logger = logger;
        }

        /// <summary>
        /// Method for creating books
        /// </summary>
        /// <param name="bookDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Book> CreateBook(BookDto bookDto)
        {
            try
            {
                var book = new Book
                {
                    Title = bookDto.Title,
                    Author = bookDto.Author,
                    ISBN = bookDto.ISBN,
                    TotalCopies = bookDto.Copies,
                    AvailableCopies = bookDto.Copies
                };
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating new books. The exception is {}", ex);
                throw;
            }
        }

        /// <summary>
        /// Method for deleting books
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DeleteBook(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                    throw new NotFoundException("Book Not found");

                var hasActiveBorrows = await _context.BorrowRecords
                    .AnyAsync(br => br.BookId == id && !br.IsReturned);

                if (hasActiveBorrows)
                    throw new BooksCanNotDeleteException("Cannot delete book with active borrows");

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting book. The exception is {}", ex);
                throw;
            }
        }

        /// <summary>
        /// Method for getting books based on id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Book> GetBook(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    throw new NotFoundException("Book Not found");
                }
                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching book record. The exception is {}", ex);
                throw;
            }
        }

        /// <summary>
        /// Method for fetching all book record.
        /// TO DO: Needs to implement pagination
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<Book>> GetBooks()
        {
            try
            {
                return await _context.Books.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching books {}", ex);
                throw;
            }
        }

        /// <summary>
        /// Method for updating book records
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDto"></param>
        /// <returns>true/false</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task UpdateBook(int id, [FromBody] BookDto bookDto)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                    throw new NotFoundException("Book Not found");

                book.Title = bookDto.Title;
                book.Author = bookDto.Author;
                book.ISBN = bookDto.ISBN;

                var copyDifference = bookDto.Copies - book.TotalCopies;
                book.TotalCopies = bookDto.Copies;
                book.AvailableCopies += copyDifference;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating books {}", ex);
                throw;
            }
        }
    }
}
