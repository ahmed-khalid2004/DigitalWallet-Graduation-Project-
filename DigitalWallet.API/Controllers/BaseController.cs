using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DigitalWallet.Application.Common;

namespace DigitalWallet.API.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Retrieves the current authenticated user's ID from JWT claims
        /// </summary>
        /// <returns>User GUID from NameIdentifier claim</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid");
            }

            return userId;
        }

        /// <summary>
        /// Retrieves the current authenticated user's role from JWT claims
        /// </summary>
        /// <returns>Role claim value or null if not present</returns>
        protected string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        protected bool HasRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Wraps a successful ServiceResult into an appropriate HTTP response.
        /// </summary>
        /// <typeparam name="T">Type of data in ServiceResult</typeparam>
        /// <param name="result">Service result to handle</param>
        /// <param name="successMessage">Optional custom success message</param>
        /// <returns>200 OK with data or 400 Bad Request with errors</returns>
        protected ActionResult<ApiResponse<T>> HandleResult<T>(ServiceResult<T> result, string? successMessage = null)
        {
            if (!result.IsSuccess)
                return BadRequest(
     ApiResponse<T>.ErrorResponse(
         string.Join(" | ", result.Errors ?? new List<string> { "Unknown error" })
     )
 );


            return Ok(ApiResponse<T>.SuccessResponse(result.Data!, result.Message ?? "Success"));
        }

        /// <summary>
        /// Wraps a successful ServiceResult into a CreatedAtAction response (201).
        /// </summary>
        /// <typeparam name="T">Type of data in ServiceResult</typeparam>
        /// <param name="result">Service result to handle</param>
        /// <param name="actionName">Name of the action to generate location header</param>
        /// <param name="routeValues">Route values for the location header</param>
        /// <param name="successMessage">Optional custom success message</param>
        /// <returns>201 Created with location header or 400 Bad Request with errors</returns>
        protected ActionResult<ApiResponse<T>> HandleCreatedResult<T>(
            ServiceResult<T> result,
            string actionName,
            object? routeValues = null,
            string? successMessage = null)
        {
            if (!result.IsSuccess)
                return BadRequest(
    ApiResponse<T>.ErrorResponse(
        string.Join(" | ", result.Errors ?? new List<string> { "Unknown error" })
    )
);

            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.SuccessResponse(result.Data!, result.Message ?? "Success"));
        }

        /// <summary>
        /// Creates a Forbidden response with a custom message
        /// </summary>
        /// <param name="message">Forbidden message</param>
        /// <returns>403 Forbidden with message</returns>
        protected ActionResult Forbid(string message)
        {
            return new ObjectResult(ApiResponse<object>.ErrorResponse(message))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}