using LibraryManagement.Models;

namespace LibraryManagement.Service
{
    /**
     * Interface for token related operations
     */
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
