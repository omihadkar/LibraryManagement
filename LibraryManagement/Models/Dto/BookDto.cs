namespace LibraryManagement.Models.Dto
{
    /// <summary>
    /// Book DTO class used while api intercation. 
    /// </summary>
    public class BookDto
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int Copies { get; set; }
    }
}
