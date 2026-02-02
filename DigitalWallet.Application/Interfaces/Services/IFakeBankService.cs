using DigitalWallet.Application.DTOs.FakeBank;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IFakeBankService
    {
        Task<ServiceResult<FakeBankTransactionDto>> DepositAsync(DepositRequestDto request);
        Task<ServiceResult<FakeBankTransactionDto>> WithdrawAsync(WithdrawRequestDto request);
    }
}