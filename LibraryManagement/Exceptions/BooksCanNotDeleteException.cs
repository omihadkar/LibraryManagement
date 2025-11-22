namespace LibraryManagement.Exceptions
{
    public class BooksCanNotDeleteException : Exception
    {
        public BooksCanNotDeleteException() { }
        public BooksCanNotDeleteException(string message) : base(message) { }

    }
}
