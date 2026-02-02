using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IBillPaymentRepository
    {
        Task<BillPayment?> GetByIdAsync(Guid id);
        Task<IEnumerable<BillPayment>> GetByUserIdAsync(Guid userId);
        Task<BillPayment> AddAsync(BillPayment billPayment);
        Task UpdateAsync(BillPayment billPayment);
    }
}