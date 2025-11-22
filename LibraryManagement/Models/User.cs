namespace LibraryManagement.Models
{
    /// <summary>
    /// Model class for the user
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Role can be "Librarian" or "Client"

        // declaring BorrowRecords just for establising foreign constraints in the context
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}
