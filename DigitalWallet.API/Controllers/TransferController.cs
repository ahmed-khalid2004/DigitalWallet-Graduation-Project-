using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Transfer;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Handles money transfer operations between wallets
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : BaseController
    {
        private readonly ITransferService _transferService;
        private readonly IWalletService _walletService;
        private readonly ILogger<TransferController> _logger;

        public TransferController(
            ITransferService transferService,
            IWalletService walletService,
            ILogger<TransferController> logger)
        {
            _transferService = transferService;
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Sends money from sender's wallet to receiver's wallet
        /// Requires valid OTP and sender must own the source wallet
        /// </summary>
        /// <param name="request">Transfer details including sender wallet, receiver identifier, amount, and OTP</param>
        /// <returns>Transfer confirmation with details</returns>
        /// <response code="200">Transfer completed successfully</response>
        /// <response code="400">Validation errors, invalid OTP, or insufficient balance</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">Sender wallet does not belong to the user</response>
        /// <response code="404">Receiver not found or doesn't have matching wallet</response>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<TransferResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<TransferResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<TransferResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TransferResponseDto>>> SendMoney([FromBody] SendMoneyRequestDto request)
        {
            // ── Input validation ──────────────────────────────────────────────
            if (request.SenderWalletId == Guid.Empty)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Sender wallet ID is required."));

            if (string.IsNullOrWhiteSpace(request.ReceiverPhoneOrEmail))
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Receiver phone or email is required."));

            if (request.Amount <= 0)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Transfer amount must be greater than zero."));

            if (string.IsNullOrWhiteSpace(request.OtpCode) || request.OtpCode.Length != 6)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("A valid 6-digit OTP code is required."));

            if (!request.OtpCode.All(char.IsDigit))
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("OTP code must contain only digits."));

            // ── Ownership guard ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            var walletResult = await _walletService.GetWalletByIdAsync(request.SenderWalletId);

            if (!walletResult.IsSuccess)
                return BadRequest(ApiResponse<TransferResponseDto>.ErrorResponse("Sender wallet not found."));

            if (walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to transfer from WalletId {WalletId} owned by {OwnerId}.",
                    currentUserId, request.SenderWalletId, walletResult.Data.UserId);
                return Forbid("You can only transfer money from your own wallets.");
            }

            // ── Execute transfer ───────────────────────────────────────────────
            _logger.LogInformation("Transfer initiated by UserId: {UserId}, Amount: {Amount}, Receiver: {Receiver}",
                currentUserId, request.Amount, request.ReceiverPhoneOrEmail);

            var result = await _transferService.SendMoneyAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Transfer failed for UserId: {UserId}. Errors: {Errors}",
                    currentUserId, string.Join(" | ", result.Errors));
                return BadRequest(
    ApiResponse<TransferResponseDto>.ErrorResponse(
        string.Join(" | ", result.Errors ?? new List<string> { "Transfer failed" })
    )
);

            }

            _logger.LogInformation("Transfer succeeded for UserId: {UserId}, TransferId: {TransferId}",
                currentUserId, result.Data?.TransferId);

            return Ok(ApiResponse<TransferResponseDto>.SuccessResponse(result.Data!, result.Message ?? "Transfer completed successfully"));
        }

        /// <summary>
        /// Retrieves transfer history for a specific wallet with pagination
        /// </summary>
        /// <param name="walletId">Wallet identifier</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>Paginated list of transfers</returns>
        /// <response code="200">Transfer history retrieved successfully</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">Wallet does not belong to the user</response>
        /// <response code="404">Wallet not found</response>
        [HttpGet("history/{walletId}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TransferDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TransferDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TransferDto>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<TransferDto>>>> GetTransferHistory(
            Guid walletId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                return BadRequest(ApiResponse<PaginatedResult<TransferDto>>.ErrorResponse("Page number must be greater than 0."));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<PaginatedResult<TransferDto>>.ErrorResponse("Page size must be between 1 and 100."));

            // ── Ownership guard ──────────────────────────────────────────────
            var currentUserId = GetCurrentUserId();
            var walletResult = await _walletService.GetWalletByIdAsync(walletId);

            if (!walletResult.IsSuccess)
                return NotFound(ApiResponse<PaginatedResult<TransferDto>>.ErrorResponse("Wallet not found."));

            if (walletResult.Data!.UserId != currentUserId)
            {
                _logger.LogWarning("UserId {CurrentId} attempted to view transfer history for WalletId {WalletId} owned by {OwnerId}.",
                    currentUserId, walletId, walletResult.Data.UserId);
                return Forbid("You can only view transfer history for your own wallets.");
            }

            _logger.LogInformation("Fetching transfer history for WalletId: {WalletId}, Page: {Page}, Size: {Size}",
                walletId, pageNumber, pageSize);

            var result = await _transferService.GetTransferHistoryAsync(walletId, pageNumber, pageSize);
            return HandleResult(result);
        }
    }
}