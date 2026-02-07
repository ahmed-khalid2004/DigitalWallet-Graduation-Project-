using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalWallet.Application.DTOs.Notification;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Manages user notifications
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all notifications for the authenticated user with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <returns>List of notifications</returns>
        /// <response code="200">Notifications retrieved successfully</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="401">User not authenticated</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageNumber < 1)
                return BadRequest(ApiResponse<IEnumerable<NotificationDto>>.ErrorResponse("Page number must be greater than 0."));

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<IEnumerable<NotificationDto>>.ErrorResponse("Page size must be between 1 and 100."));

            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching notifications for UserId: {UserId}, Page: {Page}, Size: {Size}",
                userId, pageNumber, pageSize);

            var result = await _notificationService.GetUserNotificationsAsync(userId, pageNumber, pageSize);
            return HandleResult(result);
        }

        /// <summary>
        /// Marks a specific notification as read
        /// </summary>
        /// <param name="id">Notification identifier</param>
        /// <returns>Confirmation of marking as read</returns>
        /// <response code="200">Notification marked as read</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">Notification not found</response>
        [HttpPatch("{id}/read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid id)
        {
            _logger.LogInformation("Marking notification {NotificationId} as read", id);

            var result = await _notificationService.MarkAsReadAsync(id);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves the count of unread notifications
        /// </summary>
        /// <returns>Count of unread notifications</returns>
        /// <response code="200">Count retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Fetching unread notification count for UserId: {UserId}", userId);

            var result = await _notificationService.GetUnreadCountAsync(userId);
            return HandleResult(result);
        }
    }
}