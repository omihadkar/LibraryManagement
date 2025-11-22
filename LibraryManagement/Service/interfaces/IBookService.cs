using LibraryManagement.Models;
using LibraryManagement.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Service.interfaces
{
    /// <summary>
    /// Interface is being used for handling books related operations.
    /// </summary>
    public interface IBookService
    {
        public Task<IEnumerable<Book>> GetBooks();
        public Task<Book> GetBook(int id);
        public Task<Book> CreateBook(BookDto bookDto);
        public Task UpdateBook(int id, BookDto bookDto);
        public Task DeleteBook(int id);
    }
}
