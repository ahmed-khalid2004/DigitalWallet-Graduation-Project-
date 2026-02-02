using DigitalWallet.Application.DTOs.Wallet;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<ServiceResult<WalletDto>> GetWalletByIdAsync(Guid walletId);
        Task<ServiceResult<IEnumerable<WalletDto>>> GetUserWalletsAsync(Guid userId);
        Task<ServiceResult<WalletDto>> CreateWalletAsync(CreateWalletRequestDto request);
        Task<ServiceResult<WalletBalanceDto>> GetBalanceAsync(Guid walletId);
    }
}