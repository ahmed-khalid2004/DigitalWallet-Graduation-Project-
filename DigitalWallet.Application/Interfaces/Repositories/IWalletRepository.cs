using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByIdAsync(Guid id);
        Task<IEnumerable<Wallet>> GetByUserIdAsync(Guid userId);
        Task<Wallet?> GetByUserIdAndCurrencyAsync(Guid userId, string currency);
        Task<Wallet> AddAsync(Wallet wallet);
        Task UpdateAsync(Wallet wallet);
        Task DeleteAsync(Guid id);
    }
}