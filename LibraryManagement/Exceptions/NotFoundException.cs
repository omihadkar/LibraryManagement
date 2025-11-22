namespace LibraryManagement.Exceptions
{
    /// <summary>
    /// Custom exception class for Not found exception.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException() { }
        public NotFoundException(string message) : base(message) { }

    }
}
