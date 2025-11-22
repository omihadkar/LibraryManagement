using LibraryManagement.Models;

namespace LibraryManagement.Service
{
    /// <summary>
    /// Interface for token related operations
    /// </summary>
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
