using LibraryManagement.Context;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Service.interfaces;
using LibraryManagement.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.Service
{
    /// <summary>
    /// Class for managing borrow books related activities.
    /// </summary>
    public class BorrowService : IBorrowService
    {
        private readonly LibraryContext context;
        private readonly ILogger<BorrowService> logger;
        public BorrowService(LibraryContext context, ILogger<BorrowService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        /// <summary>
        /// Method support borrwing book funcitonality
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="BadHttpRequestException"></exception>
        public async Task BorrowBook(int bookId, int userId)
        {
            try
            {                
                var book = await context.Books.FindAsync(bookId);

                if (book == null)
                    throw new NotFoundException("Book not found");

                if (book.AvailableCopies <= 0)
                    throw new BadHttpRequestException("No copies available");

                var hasUnreturnedBook = await context.BorrowRecords
                    .AnyAsync(br => br.UserId == userId && br.BookId == bookId && !br.IsReturned);

                if (hasUnreturnedBook)
                    throw new BadHttpRequestException("You already have this book borrowed");

                var borrowRecord = new BorrowRecord
                {
                    UserId = userId,
                    BookId = bookId,
                    BorrowDate = DateTime.UtcNow,
                    IsReturned = false
                };

                book.AvailableCopies--;
                context.BorrowRecords.Add(borrowRecord);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error while borrowing book for bookId {} and userId {}. The exception is {}", [bookId, userId, ex]);
                throw;
            }

        }

        /// <summary>
        /// Gets the record of all users who has borrowed the books
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<object>> GetAllBorrows()
        {
            try
            {
                var borrows = await context.BorrowRecords
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
                return borrows;
            }
            catch (Exception ex)
            {
                logger.LogError("Error while fetching borrow records. The exception is {}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get borrow records per user id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<object>> GetMyBorrows(int userId)
        {
            try
            {
                var borrows = await context.BorrowRecords
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
                return borrows;
            }
            catch (Exception ex)
            {
                logger.LogError("Error while fetching borrow records for user id {}. The exception is {}", [userId, ex]);
                throw;
            }

        }

        /// <summary>
        /// Return book functionality based on borrowid and user id.
        /// </summary>
        /// <param name="borrowId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task ReturnBook(int borrowId, ClaimsPrincipal user)
        {
            try
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var borrowRecord = await context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == borrowId);

                if (borrowRecord == null)
                    throw new NotFoundException("Borrow record not found");

                // To handle case wherein apart from librarian nobody should return others book.
                if (borrowRecord.UserId != userId && !user.IsInRole(Constants.LIBRARIAN_ROLE))
                    throw new ForbiddenActionException("Returning others book is not allowed. Contact librarian.");

                if (borrowRecord.IsReturned)
                    throw new BadHttpRequestException("Book already returned");                

                borrowRecord.IsReturned = true;
                borrowRecord.ReturnDate = DateTime.UtcNow;
                borrowRecord.Book.AvailableCopies++;

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error while returning book for borrow id {}. " +
                    "The exception is {}", [borrowId, ex]);
                throw;
            }

        }
    }
}
