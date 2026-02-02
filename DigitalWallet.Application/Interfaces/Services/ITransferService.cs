using DigitalWallet.Application.DTOs.Transfer;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface ITransferService
    {
        Task<ServiceResult<TransferResponseDto>> SendMoneyAsync(SendMoneyRequestDto request);
        Task<ServiceResult<IEnumerable<TransferDto>>> GetUserTransfersAsync(Guid userId);
    }
}