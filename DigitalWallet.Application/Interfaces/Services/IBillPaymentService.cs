using DigitalWallet.Application.DTOs.BillPayment;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IBillPaymentService
    {
        Task<ServiceResult<IEnumerable<BillerDto>>> GetAllBillersAsync();
        Task<ServiceResult<BillPaymentDto>> PayBillAsync(Guid userId, PayBillRequestDto request);
        Task<ServiceResult<IEnumerable<BillPaymentDto>>> GetUserBillPaymentsAsync(Guid userId);
    }
}