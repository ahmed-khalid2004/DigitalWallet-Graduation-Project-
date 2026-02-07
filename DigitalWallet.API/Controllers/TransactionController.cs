using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Transaction;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Manages transaction history and details
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly IWalletService _walletService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionService transactionService,
            IWalletService walletService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves paginated transaction history for a specific wallet
        /// </summary>
        /// <param name="walletId">Wallet identifier</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>Paginated list of transactions</returns>
        /// <response code="200">Transaction history retrieved successfully</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">Wallet does not belong to the user</response>
        /// <response code="404">Wallet not found</response>
        [HttpGet("wallet/{walletId}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TransactionHistoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TransactionHistoryDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TransactionHistoryDto>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<TransactionHistoryDto>>>> GetTransactionHistory(
            Guid walletId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                return BadRequest(ApiResponse<PaginatedResult<TransactionHistoryDto>>.ErrorResponse("Page number must be greater than 0."));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<PaginatedResult<TransactionHistoryDto>>.ErrorResponse("Page size must be between 1 and 100."));

            // ── Ownership guard ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            var walletResult = await _walletService.GetWalletByIdAsync(walletId);

            if (!walletResult.IsSuccess)
                return NotFound(ApiResponse<PaginatedResult<TransactionHistoryDto>>.ErrorResponse("Wallet not found."));

            if (walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to view transactions for WalletId {WalletId} owned by {OwnerId}.",
                    currentUserId, walletId, walletResult.Data.UserId);
                return Forbid("You can only view transactions for your own wallets.");
            }

            _logger.LogInformation("Fetching transaction history for WalletId: {WalletId}, Page: {Page}, Size: {Size}",
                walletId, pageNumber, pageSize);

            var result = await _transactionService.GetTransactionHistoryAsync(walletId, pageNumber, pageSize);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a specific transaction by ID
        /// </summary>
        /// <param name="id">Transaction identifier</param>
        /// <returns>Transaction details</returns>
        /// <response code="200">Transaction retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">Transaction does not belong to the user</response>
        /// <response code="404">Transaction not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransactionById(Guid id)
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Fetching transaction {TransactionId} for UserId: {UserId}", id, currentUserId);

            var result = await _transactionService.GetTransactionByIdAsync(id);

            if (!result.IsSuccess)
                return HandleResult(result);

            // Verify the transaction's wallet belongs to the current user
            var walletResult = await _walletService.GetWalletByIdAsync(result.Data!.WalletId);
            if (walletResult.IsSuccess && walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {UserId} attempted to access transaction {TransactionId} from another user's wallet",
                    currentUserId, id);
                return Forbid("You can only view your own transactions.");
            }

            return HandleResult(result);
        }
    }
}