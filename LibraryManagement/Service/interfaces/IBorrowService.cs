using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Service.interfaces
{
    /// <summary>
    /// Interface for managing borrow books related activities.
    /// </summary>
    public interface IBorrowService
    {
        Task BorrowBook(int bookId);
        Task ReturnBook(int borrowId);
        Task<IEnumerable<object>> GetMyBorrows();
        Task<IEnumerable<object>> GetAllBorrows();
    }
}
