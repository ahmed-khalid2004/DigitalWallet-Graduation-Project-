using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IAdminService
    {
        Task<ServiceResult<IEnumerable<UserManagementDto>>> GetAllUsersAsync();
        Task<ServiceResult<IEnumerable<FraudLogDto>>> GetFraudLogsAsync();
    }
}