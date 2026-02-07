using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Wallet;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Manages digital wallet operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : BaseController
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new wallet for the authenticated user
        /// </summary>
        /// <param name="request">Currency code for the new wallet</param>
        /// <returns>Newly created wallet details</returns>
        /// <response code="201">Wallet created successfully</response>
        /// <response code="400">Wallet already exists for this currency</response>
        /// <response code="401">User not authenticated</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WalletDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<WalletDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<WalletDto>>> CreateWallet([FromBody] CreateWalletRequestDto request)
        {
            _logger.LogInformation("Creating wallet for Currency: {Currency}", request.CurrencyCode);

            var result = await _walletService.CreateWalletAsync(request);
            return HandleCreatedResult(result, nameof(GetWalletById), new { id = result.Data?.Id });
        }

        /// <summary>
        /// Retrieves all wallets belonging to the authenticated user
        /// </summary>
        /// <returns>Collection of user's wallets</returns>
        /// <response code="200">Wallets retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        [HttpGet("my-wallets")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WalletDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<WalletDto>>>> GetMyWallets()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching wallets for UserId: {UserId}", userId);

            var result = await _walletService.GetUserWalletsAsync(userId);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a specific wallet by ID
        /// </summary>
        /// <param name="id">Wallet identifier</param>
        /// <returns>Wallet details</returns>
        /// <response code="200">Wallet retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">Wallet does not belong to the user</response>
        /// <response code="404">Wallet not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<WalletDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<WalletDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<WalletDto>>> GetWalletById(Guid id)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching wallet {WalletId} for UserId: {UserId}", id, userId);

            var result = await _walletService.GetWalletByIdAsync(id);

            if (!result.IsSuccess)
                return HandleResult(result);

            // Verify ownership
            if (result.Data!.UserId != userId)
            {
                _logger.LogWarning("UserId {UserId} attempted to access wallet {WalletId} owned by {OwnerId}",
                    userId, id, result.Data.UserId);
                return Forbid("You can only access your own wallets.");
            }

            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves the wallet balance by wallet ID
        /// </summary>
        /// <param name="walletId">Wallet identifier</param>
        /// <returns>Wallet balance information</returns>
        /// <response code="200">Balance retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">Wallet does not belong to the user</response>
        /// <response code="404">Wallet not found</response>
        [HttpGet("{walletId}/balance")]
        [ProducesResponseType(typeof(ApiResponse<WalletBalanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<WalletBalanceDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<WalletBalanceDto>>> GetBalance(Guid walletId)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching balance for WalletId: {WalletId}", walletId);

            // First verify ownership
            var walletResult = await _walletService.GetWalletByIdAsync(walletId);
            if (!walletResult.IsSuccess)
                return HandleResult<WalletBalanceDto>(ServiceResult<WalletBalanceDto>.Failure("Wallet not found"));

            if (walletResult.Data!.UserId != userId)
            {
                _logger.LogWarning("UserId {UserId} attempted to access wallet {WalletId} balance owned by {OwnerId}",
                    userId, walletId, walletResult.Data.UserId);
                return Forbid("You can only view your own wallet balance.");
            }

            var result = await _walletService.GetWalletBalanceAsync(walletId);
            return HandleResult(result);
        }
    }
}