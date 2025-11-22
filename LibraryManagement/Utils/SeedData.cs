using LibraryManagement.Context;
using LibraryManagement.Models;

namespace LibraryManagement.Utils
{
    /// <summary>
    /// Class helps to seed initial data to start api.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Method initializes data for the DB context
        /// </summary>
        /// <param name="context"></param>
        public static void Initialize(LibraryContext context)
        {
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { Id = 1, Username = "librarian", Password = "admin123", Role = "Librarian" },
                    new User { Id = 2, Username = "client1", Password = "pass123", Role = "Client" },
                    new User { Id = 3, Username = "client2", Password = "pass123", Role = "Client" },
                    new User { Id = 4, Username = "client3", Password = "pass123", Role = "Client" },
                    new User { Id = 5, Username = "client4", Password = "pass123", Role = "Client" },
                    new User { Id = 6, Username = "client5", Password = "pass123", Role = "Client" }
                );
                context.Books.AddRange(
                    new Book
                    {
                        Id = 1,
                        ISBN = "98754215632",
                        Author = "Gayle Laakmann McDowell",
                        Title = "Cracking the Coding Interview",
                        AvailableCopies = 8,
                        TotalCopies = 10
                    },
                    new Book
                    {
                        Id = 2,
                        ISBN = "98754215632",
                        Author = "Gayle Laakmann McDowell",
                        Title = "Cracking the Tech Career",
                        AvailableCopies = 10,
                        TotalCopies = 10
                    },
                    new Book
                    {
                        Id = 3,
                        ISBN = "98754215632",
                        Author = "Gayle Laakmann McDowell",
                        Title = "Cracking the PM Career",
                        AvailableCopies = 7,
                        TotalCopies = 10
                    },
                    new Book
                    {
                        Id = 4,
                        ISBN = "98754215632",
                        Author = "Gayle Laakmann McDowell",
                        Title = "Cracking the PM Interview",
                        AvailableCopies = 9,
                        TotalCopies = 10
                    });
                context.BorrowRecords.AddRange(
                        new BorrowRecord() { Id = 5, BorrowDate = DateTime.Now.AddDays(-2), IsReturned = false, UserId = 5 ,BookId=3},
                        new BorrowRecord() { Id = 6, BorrowDate = DateTime.Now.AddDays(-10), IsReturned = false, UserId = 6, BookId = 3 },
                        new BorrowRecord() { Id = 3, BorrowDate = DateTime.Now.AddDays(-2), IsReturned = false, UserId = 2, BookId = 3 },
                        new BorrowRecord() { Id = 4, BorrowDate = DateTime.Now.AddDays(-10), IsReturned = false, UserId = 3, BookId = 1 },
                        new BorrowRecord() { Id = 1, BorrowDate = DateTime.Now.AddDays(-2), IsReturned = false, UserId = 2, BookId = 1 },
                        new BorrowRecord() { Id = 2, BorrowDate = DateTime.Now.AddDays(-10), IsReturned = false, UserId = 3, BookId = 4 }
                        );
                context.SaveChanges();
            }
        }
    }
}
