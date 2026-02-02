using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Transaction;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;

namespace DigitalWallet.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<TransactionDto>> GetTransactionByIdAsync(Guid transactionId)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(transactionId);

                if (transaction == null)
                    return ServiceResult<TransactionDto>.Failure("Transaction not found");

                var transactionDto = _mapper.Map<TransactionDto>(transaction);
                return ServiceResult<TransactionDto>.Success(transactionDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<TransactionDto>.Failure($"Error retrieving transaction: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResult<TransactionDto>>> GetWalletTransactionsAsync(
            Guid walletId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var transactions = await _transactionRepository.GetByWalletIdAsync(walletId, pageNumber, pageSize);
                var totalCount = await _transactionRepository.GetCountByWalletIdAsync(walletId);

                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);
                var paginatedResult = PaginatedResult<TransactionDto>.Create(
                    transactionDtos, totalCount, pageNumber, pageSize);

                return ServiceResult<PaginatedResult<TransactionDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResult<TransactionDto>>.Failure(
                    $"Error retrieving transactions: {ex.Message}");
            }
        }
    }
}