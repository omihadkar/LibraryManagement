using LibraryManagement.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Service.interfaces
{
    /// <summary>
    /// Interface for Authentication related actions.
    /// </summary>
    public interface IAuthService
    {
        Task<string> Login(LoginDto loginDto);
        Task Register(RegisterDto registerDto);
    }
}
