using LibraryManagement.Context;
using LibraryManagement.Service.interfaces;

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

        public Task BorrowBook(int bookId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<object>> GetAllBorrows()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<object>> GetMyBorrows()
        {
            throw new NotImplementedException();
        }

        public Task ReturnBook(int borrowId)
        {
            throw new NotImplementedException();
        }
    }
}
