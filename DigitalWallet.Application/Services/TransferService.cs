using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Transfer;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly ITransferRepository _transferRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public TransferService(
            ITransferRepository transferRepository,
            IWalletRepository walletRepository,
            IUserRepository userRepository,
            ITransactionRepository transactionRepository,
            IOtpCodeRepository otpRepository,
            INotificationRepository notificationRepository,
            IMapper mapper)
        {
            _transferRepository = transferRepository;
            _walletRepository = walletRepository;
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
            _otpRepository = otpRepository;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<TransferResponseDto>> SendMoneyAsync(SendMoneyRequestDto request)
        {
            try
            {
                // 1. Verify OTP
                var senderWallet = await _walletRepository.GetByIdAsync(request.SenderWalletId);
                if (senderWallet == null)
                    return ServiceResult<TransferResponseDto>.Failure("Sender wallet not found");

                var otpValid = await _otpRepository.GetValidOtpAsync(
                    senderWallet.UserId, request.OtpCode, OtpType.Transfer);

                if (otpValid == null || otpValid.IsUsed)
                    return ServiceResult<TransferResponseDto>.Failure("Invalid or expired OTP");

                // 2. Find receiver
                var receiver = await _userRepository.GetByEmailOrPhoneAsync(request.ReceiverPhoneOrEmail);
                if (receiver == null)
                    return ServiceResult<TransferResponseDto>.Failure("Receiver not found");

                var receiverWallet = (await _walletRepository.GetByUserIdAsync(receiver.Id)).FirstOrDefault();
                if (receiverWallet == null)
                    return ServiceResult<TransferResponseDto>.Failure("Receiver wallet not found");

                // 3. Validate amount
                if (senderWallet.Balance < request.Amount)
                    return ServiceResult<TransferResponseDto>.Failure("Insufficient balance");

                if (request.Amount > senderWallet.DailyLimit)
                    return ServiceResult<TransferResponseDto>.Failure("Amount exceeds daily limit");

                // 4. Perform transfer
                senderWallet.Balance -= request.Amount;
                receiverWallet.Balance += request.Amount;

                await _walletRepository.UpdateAsync(senderWallet);
                await _walletRepository.UpdateAsync(receiverWallet);

                // 5. Create transfer record
                var transfer = new Transfer
                {
                    SenderWalletId = senderWallet.Id,
                    ReceiverWalletId = receiverWallet.Id,
                    Amount = request.Amount,
                    CurrencyCode = senderWallet.CurrencyCode,
                    Status = TransactionStatus.Success
                };

                await _transferRepository.AddAsync(transfer);

                // 6. Create transaction records
                var senderTransaction = new Transaction
                {
                    WalletId = senderWallet.Id,
                    Type = TransactionType.Transfer,
                    Amount = -request.Amount,
                    CurrencyCode = senderWallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = request.Description,
                    Reference = transfer.Id.ToString()
                };

                var receiverTransaction = new Transaction
                {
                    WalletId = receiverWallet.Id,
                    Type = TransactionType.Transfer,
                    Amount = request.Amount,
                    CurrencyCode = receiverWallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = request.Description,
                    Reference = transfer.Id.ToString()
                };

                await _transactionRepository.AddAsync(senderTransaction);
                await _transactionRepository.AddAsync(receiverTransaction);

                // 7. Mark OTP as used
                otpValid.IsUsed = true;
                await _otpRepository.UpdateAsync(otpValid);

                // 8. Send notifications
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = senderWallet.UserId,
                    Title = "Money Sent",
                    Body = $"You sent {request.Amount} {senderWallet.CurrencyCode} to {receiver.FullName}",
                    Type = NotificationType.Transaction,
                    IsRead = false
                });

                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = receiver.Id,
                    Title = "Money Received",
                    Body = $"You received {request.Amount} {receiverWallet.CurrencyCode}",
                    Type = NotificationType.Transaction,
                    IsRead = false
                });

                var response = new TransferResponseDto
                {
                    TransferId = transfer.Id,
                    ReceiverName = receiver.FullName,
                    Amount = request.Amount,
                    CurrencyCode = senderWallet.CurrencyCode,
                    Status = "Success",
                    TransferredAt = DateTime.UtcNow
                };

                return ServiceResult<TransferResponseDto>.Success(response, "Transfer successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<TransferResponseDto>.Failure($"Transfer failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<TransferDto>>> GetUserTransfersAsync(Guid userId)
        {
            try
            {
                var wallets = await _walletRepository.GetByUserIdAsync(userId);
                var allTransfers = new List<Transfer>();

                foreach (var wallet in wallets)
                {
                    var transfers = await _transferRepository.GetByWalletIdAsync(wallet.Id);
                    allTransfers.AddRange(transfers);
                }

                var transferDtos = _mapper.Map<IEnumerable<TransferDto>>(allTransfers);
                return ServiceResult<IEnumerable<TransferDto>>.Success(transferDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<TransferDto>>.Failure($"Error retrieving transfers: {ex.Message}");
            }
        }
    }
}