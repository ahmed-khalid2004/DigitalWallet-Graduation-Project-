using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface ITransferRepository
    {
        Task<Transfer?> GetByIdAsync(Guid id);
        Task<IEnumerable<Transfer>> GetByWalletIdAsync(Guid walletId);
        Task<Transfer> AddAsync(Transfer transfer);
        Task UpdateAsync(Transfer transfer);
    }
}