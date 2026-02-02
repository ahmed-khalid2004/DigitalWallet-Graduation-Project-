using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IMoneyRequestRepository
    {
        Task<MoneyRequest?> GetByIdAsync(Guid id);
        Task<IEnumerable<MoneyRequest>> GetSentRequestsAsync(Guid userId);
        Task<IEnumerable<MoneyRequest>> GetReceivedRequestsAsync(Guid userId);
        Task<MoneyRequest> AddAsync(MoneyRequest request);
        Task UpdateAsync(MoneyRequest request);
        Task DeleteAsync(Guid id);
    }
}