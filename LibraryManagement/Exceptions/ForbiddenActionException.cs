namespace LibraryManagement.Exceptions
{
    /// <summary>
    /// Custom exception class for Forbidden exception.
    /// </summary>
    public class ForbiddenActionException : Exception
    {
        public ForbiddenActionException() { }
        public ForbiddenActionException(string message) : base(message) { }

    }
}
