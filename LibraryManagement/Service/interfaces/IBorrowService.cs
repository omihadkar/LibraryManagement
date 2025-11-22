using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryManagement.Service.interfaces
{
    /// <summary>
    /// Interface for managing borrow books related activities.
    /// </summary>
    public interface IBorrowService
    {
        Task BorrowBook(int bookId, int userId);
        Task ReturnBook(int borrowId, ClaimsPrincipal user);
        Task<IEnumerable<object>> GetMyBorrows(int userId);
        Task<IEnumerable<object>> GetAllBorrows();
    }
}
