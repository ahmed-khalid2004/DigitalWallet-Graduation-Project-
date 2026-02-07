using DigitalWallet.API.Filters;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.DTOs.Wallet;
using DigitalWallet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Administrative operations for managing users, wallets, and system monitoring
    /// Requires admin authentication with appropriate role
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [RoleAuthorize("SuperAdmin", "Support")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminService adminService,
            IUserService userService,
            IWalletService walletService,
            ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _userService = userService;
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all users
        /// Requires SuperAdmin or Support role
        /// </summary>
        /// <returns>List of all users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpGet("users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserManagementDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserManagementDto>>>> GetAllUsers()
        {
            _logger.LogInformation("Admin fetching all users");

            var result = await _adminService.GetAllUsersAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a specific user by ID
        /// Requires SuperAdmin or Support role
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User details</returns>
        /// <response code="200">User retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">User not found</response>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserManagementDto>>> GetUserById(Guid userId)
        {
            _logger.LogInformation("Admin fetching user: {UserId}", userId);

            var result = await _userService.GetUserByIdAsync(userId);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves all wallets for a specific user
        /// Requires SuperAdmin or Support role
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Collection of user wallets</returns>
        /// <response code="200">Wallets retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">User not found</response>
        [HttpGet("users/{userId}/wallets")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WalletDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WalletDto>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IEnumerable<WalletDto>>>> GetUserWallets(Guid userId)
        {
            _logger.LogInformation("Admin fetching wallets for user: {UserId}", userId);

            var result = await _walletService.GetUserWalletsAsync(userId);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves all fraud logs in the system
        /// Requires SuperAdmin or Support role
        /// </summary>
        /// <returns>Collection of fraud logs</returns>
        /// <response code="200">Fraud logs retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpGet("fraud-logs")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<FraudLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<FraudLogDto>>>> GetFraudLogs()
        {
            _logger.LogInformation("Admin fetching fraud logs");

            var result = await _adminService.GetFraudLogsAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves all wallets in the system
        /// Requires SuperAdmin or Support role
        /// </summary>
        /// <returns>Collection of all wallets</returns>
        /// <response code="200">Wallets retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpGet("wallets")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WalletManagementDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<WalletManagementDto>>>> GetAllWallets()
        {
            _logger.LogInformation("Admin fetching all wallets");

            var result = await _adminService.GetAllWalletsAsync();
            return HandleResult(result);
        }
    }
}