namespace LibraryManagement.Models.Dto
{
    /// <summary>
    /// Login DTO class used while api intercation. 
    /// </summary>
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
