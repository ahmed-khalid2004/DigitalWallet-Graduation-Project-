using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.BillPayment;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Services
{
    public class BillPaymentService : IBillPaymentService
    {
        private readonly IBillPaymentRepository _billPaymentRepository;
        private readonly IBillerRepository _billerRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IMapper _mapper;

        public BillPaymentService(
            IBillPaymentRepository billPaymentRepository,
            IBillerRepository billerRepository,
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            IOtpCodeRepository otpRepository,
            IMapper mapper)
        {
            _billPaymentRepository = billPaymentRepository;
            _billerRepository = billerRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _otpRepository = otpRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<BillerDto>>> GetAllBillersAsync()
        {
            try
            {
                var billers = await _billerRepository.GetAllActiveAsync();
                var billerDtos = _mapper.Map<IEnumerable<BillerDto>>(billers);

                return ServiceResult<IEnumerable<BillerDto>>.Success(billerDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<BillerDto>>.Failure($"Error retrieving billers: {ex.Message}");
            }
        }

        public async Task<ServiceResult<BillPaymentDto>> PayBillAsync(Guid userId, PayBillRequestDto request)
        {
            try
            {
                // Verify OTP
                var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
                if (wallet == null)
                    return ServiceResult<BillPaymentDto>.Failure("Wallet not found");

                var otpValid = await _otpRepository.GetValidOtpAsync(userId, request.OtpCode, OtpType.Transfer);
                if (otpValid == null || otpValid.IsUsed)
                    return ServiceResult<BillPaymentDto>.Failure("Invalid or expired OTP");

                // Verify biller
                var biller = await _billerRepository.GetByIdAsync(request.BillerId);
                if (biller == null || !biller.IsActive)
                    return ServiceResult<BillPaymentDto>.Failure("Biller not found or inactive");

                // Check balance
                if (wallet.Balance < request.Amount)
                    return ServiceResult<BillPaymentDto>.Failure("Insufficient balance");

                // Deduct amount
                wallet.Balance -= request.Amount;
                await _walletRepository.UpdateAsync(wallet);

                // Create bill payment
                var billPayment = new BillPayment
                {
                    UserId = userId,
                    WalletId = wallet.Id,
                    BillerId = biller.Id,
                    Amount = request.Amount,
                    CurrencyCode = wallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    ReceiptPath = $"/receipts/bill_{Guid.NewGuid()}.pdf" // Simplified
                };

                await _billPaymentRepository.AddAsync(billPayment);

                // Create transaction
                var transaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Bill,
                    Amount = -request.Amount,
                    CurrencyCode = wallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = $"Bill payment to {biller.Name}",
                    Reference = billPayment.Id.ToString()
                };

                await _transactionRepository.AddAsync(transaction);

                // Mark OTP as used
                otpValid.IsUsed = true;
                await _otpRepository.UpdateAsync(otpValid);

                // Load biller for mapping
                billPayment.Biller = biller;

                var billPaymentDto = _mapper.Map<BillPaymentDto>(billPayment);
                return ServiceResult<BillPaymentDto>.Success(billPaymentDto, "Bill paid successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<BillPaymentDto>.Failure($"Bill payment failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<BillPaymentDto>>> GetUserBillPaymentsAsync(Guid userId)
        {
            try
            {
                var billPayments = await _billPaymentRepository.GetByUserIdAsync(userId);
                var billPaymentDtos = _mapper.Map<IEnumerable<BillPaymentDto>>(billPayments);

                return ServiceResult<IEnumerable<BillPaymentDto>>.Success(billPaymentDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<BillPaymentDto>>.Failure(
                    $"Error retrieving bill payments: {ex.Message}");
            }
        }
    }
}