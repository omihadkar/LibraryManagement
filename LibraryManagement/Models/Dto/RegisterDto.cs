namespace LibraryManagement.Models.Dto
{
    /// <summary>
    /// Register DTO class used while api intercation. 
    /// </summary>
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
