using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IFraudLogRepository
    {
        Task<FraudLog?> GetByIdAsync(Guid id);
        Task<IEnumerable<FraudLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<FraudLog>> GetRecentLogsAsync(int hours = 24);
        Task<FraudLog> AddAsync(FraudLog fraudLog);
    }
}