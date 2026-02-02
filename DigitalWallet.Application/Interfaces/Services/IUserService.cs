using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid userId);
        Task<ServiceResult<UserDto>> GetUserByEmailAsync(string email);
    }
}