using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.FakeBank;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.Services
{
    public class FakeBankService : IFakeBankService
    {
        private readonly IFakeBankAccountRepository _bankAccountRepository;
        private readonly IFakeBankTransactionRepository _bankTransactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public FakeBankService(
            IFakeBankAccountRepository bankAccountRepository,
            IFakeBankTransactionRepository bankTransactionRepository,
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            IMapper mapper)
        {
            _bankAccountRepository = bankAccountRepository;
            _bankTransactionRepository = bankTransactionRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<FakeBankTransactionDto>> DepositAsync(DepositRequestDto request)
        {
            try
            {
                // Get or create bank account
                var bankAccount = await _bankAccountRepository.GetByUserIdAsync(request.UserId);
                if (bankAccount == null)
                    return ServiceResult<FakeBankTransactionDto>.Failure("Bank account not found");

                // Check bank balance
                if (bankAccount.Balance < request.Amount)
                    return ServiceResult<FakeBankTransactionDto>.Failure("Insufficient bank balance");

                // Get user wallet
                var wallet = (await _walletRepository.GetByUserIdAsync(request.UserId)).FirstOrDefault();
                if (wallet == null)
                    return ServiceResult<FakeBankTransactionDto>.Failure("Wallet not found");

                // Create fake bank transaction
                var bankTransaction = new FakeBankTransaction
                {
                    UserId = request.UserId,
                    Amount = request.Amount,
                    Type = "deposit",
                    Status = TransactionStatus.Pending,
                    DelaySeconds = 5 // Simulate delay
                };

                await _bankTransactionRepository.AddAsync(bankTransaction);

                // Simulate processing delay
                await Task.Delay(bankTransaction.DelaySeconds * 1000);

                // Update balances
                bankAccount.Balance -= request.Amount;
                wallet.Balance += request.Amount;

                await _bankAccountRepository.UpdateAsync(bankAccount);
                await _walletRepository.UpdateAsync(wallet);

                // Update transaction status
                bankTransaction.Status = TransactionStatus.Success;
                await _bankTransactionRepository.UpdateAsync(bankTransaction);

                // Create wallet transaction
                var walletTransaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Deposit,
                    Amount = request.Amount,
                    CurrencyCode = wallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = "Deposit from bank",
                    Reference = bankTransaction.Id.ToString()
                };

                await _transactionRepository.AddAsync(walletTransaction);

                var transactionDto = _mapper.Map<FakeBankTransactionDto>(bankTransaction);
                return ServiceResult<FakeBankTransactionDto>.Success(transactionDto, "Deposit successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<FakeBankTransactionDto>.Failure($"Deposit failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<FakeBankTransactionDto>> WithdrawAsync(WithdrawRequestDto request)
        {
            try
            {
                var wallet = (await _walletRepository.GetByUserIdAsync(request.UserId)).FirstOrDefault();
                if (wallet == null)
                    return ServiceResult<FakeBankTransactionDto>.Failure("Wallet not found");

                if (wallet.Balance < request.Amount)
                    return ServiceResult<FakeBankTransactionDto>.Failure("Insufficient wallet balance");

                var bankAccount = await _bankAccountRepository.GetByUserIdAsync(request.UserId);
                if (bankAccount == null)
                    return ServiceResult<FakeBankTransactionDto>.Failure("Bank account not found");

                var bankTransaction = new FakeBankTransaction
                {
                    UserId = request.UserId,
                    Amount = request.Amount,
                    Type = "withdraw",
                    Status = TransactionStatus.Pending,
                    DelaySeconds = 5
                };

                await _bankTransactionRepository.AddAsync(bankTransaction);

                await Task.Delay(bankTransaction.DelaySeconds * 1000);

                wallet.Balance -= request.Amount;
                bankAccount.Balance += request.Amount;

                await _walletRepository.UpdateAsync(wallet);
                await _bankAccountRepository.UpdateAsync(bankAccount);

                bankTransaction.Status = TransactionStatus.Success;
                await _bankTransactionRepository.UpdateAsync(bankTransaction);

                var walletTransaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Withdraw,
                    Amount = -request.Amount,
                    CurrencyCode = wallet.CurrencyCode,
                    Status = TransactionStatus.Success,
                    Description = "Withdraw to bank",
                    Reference = bankTransaction.Id.ToString()
                };

                await _transactionRepository.AddAsync(walletTransaction);

                var transactionDto = _mapper.Map<FakeBankTransactionDto>(bankTransaction);
                return ServiceResult<FakeBankTransactionDto>.Success(transactionDto, "Withdrawal successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<FakeBankTransactionDto>.Failure($"Withdrawal failed: {ex.Message}");
            }
        }
    }
}