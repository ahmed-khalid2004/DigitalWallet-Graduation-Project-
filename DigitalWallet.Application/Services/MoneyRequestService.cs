using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.MoneyRequest;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Services
{
    public class MoneyRequestService : IMoneyRequestService
    {
        private readonly IMoneyRequestRepository _requestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IMapper _mapper;

        public MoneyRequestService(
            IMoneyRequestRepository requestRepository,
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            ITransferRepository transferRepository,
            IOtpCodeRepository otpRepository,
            IMapper mapper)
        {
            _requestRepository = requestRepository;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transferRepository = transferRepository;
            _otpRepository = otpRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<MoneyRequestDto>> CreateRequestAsync(
            Guid fromUserId, CreateMoneyRequestDto request)
        {
            try
            {
                var toUser = await _userRepository.GetByEmailOrPhoneAsync(request.ToUserPhoneOrEmail);
                if (toUser == null)
                    return ServiceResult<MoneyRequestDto>.Failure("User not found");

                if (toUser.Id == fromUserId)
                    return ServiceResult<MoneyRequestDto>.Failure("Cannot request money from yourself");

                var moneyRequest = new MoneyRequest
                {
                    FromUserId = fromUserId,
                    ToUserId = toUser.Id,
                    Amount = request.Amount,
                    CurrencyCode = request.CurrencyCode,
                    Status = MoneyRequestStatus.Pending
                };

                await _requestRepository.AddAsync(moneyRequest);

                // Load navigation properties for mapping
                moneyRequest.FromUser = await _userRepository.GetByIdAsync(fromUserId);
                moneyRequest.ToUser = toUser;

                var requestDto = _mapper.Map<MoneyRequestDto>(moneyRequest);
                return ServiceResult<MoneyRequestDto>.Success(requestDto, "Request created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<MoneyRequestDto>.Failure($"Error creating request: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<MoneyRequestDto>>> GetSentRequestsAsync(Guid userId)
        {
            try
            {
                var requests = await _requestRepository.GetSentRequestsAsync(userId);
                var requestDtos = _mapper.Map<IEnumerable<MoneyRequestDto>>(requests);

                return ServiceResult<IEnumerable<MoneyRequestDto>>.Success(requestDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<MoneyRequestDto>>.Failure(
                    $"Error retrieving sent requests: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<MoneyRequestDto>>> GetReceivedRequestsAsync(Guid userId)
        {
            try
            {
                var requests = await _requestRepository.GetReceivedRequestsAsync(userId);
                var requestDtos = _mapper.Map<IEnumerable<MoneyRequestDto>>(requests);

                return ServiceResult<IEnumerable<MoneyRequestDto>>.Success(requestDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<MoneyRequestDto>>.Failure(
                    $"Error retrieving received requests: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> RespondToRequestAsync(
            Guid userId, AcceptRejectRequestDto request)
        {
            try
            {
                var moneyRequest = await _requestRepository.GetByIdAsync(request.RequestId);
                if (moneyRequest == null)
                    return ServiceResult<bool>.Failure("Request not found");

                if (moneyRequest.ToUserId != userId)
                    return ServiceResult<bool>.Failure("Unauthorized to respond to this request");

                if (moneyRequest.Status != MoneyRequestStatus.Pending)
                    return ServiceResult<bool>.Failure("Request already processed");

                if (!request.Accept)
                {
                    moneyRequest.Status = MoneyRequestStatus.Rejected;
                    await _requestRepository.UpdateAsync(moneyRequest);
                    return ServiceResult<bool>.Success(true, "Request rejected");
                }

                // Verify OTP
                if (string.IsNullOrEmpty(request.OtpCode))
                    return ServiceResult<bool>.Failure("OTP required to accept request");

                var otpValid = await _otpRepository.GetValidOtpAsync(userId, request.OtpCode, OtpType.Transfer);
                if (otpValid == null || otpValid.IsUsed)
                    return ServiceResult<bool>.Failure("Invalid or expired OTP");

                // Perform transfer (simplified - should use TransferService)
                var senderWallet = (await _walletRepository.GetByUserIdAsync(userId)).FirstOrDefault();
                var receiverWallet = (await _walletRepository.GetByUserIdAsync(moneyRequest.FromUserId)).FirstOrDefault();

                if (senderWallet == null || receiverWallet == null)
                    return ServiceResult<bool>.Failure("Wallet not found");

                if (senderWallet.Balance < moneyRequest.Amount)
                    return ServiceResult<bool>.Failure("Insufficient balance");

                senderWallet.Balance -= moneyRequest.Amount;
                receiverWallet.Balance += moneyRequest.Amount;

                await _walletRepository.UpdateAsync(senderWallet);
                await _walletRepository.UpdateAsync(receiverWallet);

                // Update request status
                moneyRequest.Status = MoneyRequestStatus.Accepted;
                await _requestRepository.UpdateAsync(moneyRequest);

                // Mark OTP as used
                otpValid.IsUsed = true;
                await _otpRepository.UpdateAsync(otpValid);

                return ServiceResult<bool>.Success(true, "Request accepted and transfer completed");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error responding to request: {ex.Message}");
            }
        }
    }
}