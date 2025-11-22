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
        public BookService(LibraryContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// Method for creating books
        /// </summary>
        /// <param name="bookDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Book> CreateBook(BookDto bookDto)
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

        /// <summary>
        /// Method for deleting books
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DeleteBook(int id)
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

        /// <summary>
        /// Method for getting books based on id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Book?> GetBook(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        /// <summary>
        /// Method for fetching all book record.
        /// TO DO: Needs to implement pagination
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<Book>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        /// <summary>
        /// Method for updating book records
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDto"></param>
        /// <returns>true/false</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateBook(int id, [FromBody] BookDto bookDto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            book.Title = bookDto.Title;
            book.Author = bookDto.Author;
            book.ISBN = bookDto.ISBN;

            var copyDifference = bookDto.Copies - book.TotalCopies;
            book.TotalCopies = bookDto.Copies;
            book.AvailableCopies += copyDifference;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
