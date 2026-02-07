using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Manages user profile operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the authenticated user's profile
        /// </summary>
        /// <returns>User profile details</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">User not found</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserManagementDto>>> GetProfile()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching profile for UserId: {UserId}", userId);

            var result = await _userService.GetUserByIdAsync(userId);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User information</returns>
        /// <response code="200">User found</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">User not found</response>
        [HttpGet("find-by-email/{email}")]
        [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserManagementDto>>> FindUserByEmail(string email)
        {
            _logger.LogInformation("Finding user by email: {Email}", email);

            var result = await _userService.GetUserByEmailAsync(email);
            return HandleResult(result);
        }
    }
}