using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IAdminRepository
    {
        Task<AdminUser?> GetByIdAsync(Guid id);
        Task<AdminUser?> GetByEmailAsync(string email);
        Task<IEnumerable<AdminUser>> GetAllAsync();
        Task<AdminUser> AddAsync(AdminUser admin);
        Task UpdateAsync(AdminUser admin);
        Task DeleteAsync(Guid id);
        Task<AdminActionsLog> LogActionAsync(AdminActionsLog log);
        Task<IEnumerable<AdminActionsLog>> GetActionLogsAsync(Guid? adminId = null);
    }
}