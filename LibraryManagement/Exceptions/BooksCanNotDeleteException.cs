namespace LibraryManagement.Exceptions
{
    /// <summary>
    /// Custom exception class for "Books can not delete" (When it is borrowed.) exception.
    /// </summary>
    public class BooksCanNotDeleteException : Exception
    {
        public BooksCanNotDeleteException() { }
        public BooksCanNotDeleteException(string message) : base(message) { }

    }
}
