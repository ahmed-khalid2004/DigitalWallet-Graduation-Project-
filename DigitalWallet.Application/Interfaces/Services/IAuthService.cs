using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Common;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ServiceResult<bool>> VerifyOtpAsync(VerifyOtpRequestDto request);
        Task<ServiceResult<string>> GenerateOtpAsync(Guid userId, string type);
    }
}