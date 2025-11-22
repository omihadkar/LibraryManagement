using LibraryManagement.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Service
{
    /// <summary>
    /// Interface for Authentication related actions.
    /// </summary>
    public interface IAuthService
    {
        Task<String> Login(LoginDto loginDto);
        Task Register(RegisterDto registerDto);
    }
}
