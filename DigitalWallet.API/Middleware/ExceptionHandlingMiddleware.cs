using System.Net;
using System.Text.Json;
using DigitalWallet.Application.Common;
using DigitalWallet.Domain.Exceptions;

namespace DigitalWallet.API.Middleware
{
    /// <summary>
    /// Global exception handler that catches unhandled exceptions and returns
    /// consistent error responses in the ApiResponse format.
    /// 
    /// Exception handling strategy:
    /// - DomainException (business rule violations)       → 400 Bad Request
    /// - UnauthorizedAccessException                      → 401 Unauthorized
    /// - KeyNotFoundException / ArgumentNullException     → 404 Not Found
    /// - All other exceptions                             → 500 Internal Server Error
    /// 
    /// In production, detailed error messages are suppressed to prevent information leakage.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // ── Log the exception with full details ────────────────────────
            _logger.LogError(exception,
                "Unhandled exception occurred: {Message} | Path: {Path}",
                exception.Message,
                context.Request.Path);

            // ── Determine HTTP status code and error message ───────────────
            var (statusCode, errorMessage) = MapExceptionToResponse(exception);

            // ── Build response ─────────────────────────────────────────────
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.ErrorResponse(errorMessage);

            // In development, include stack trace for debugging
            if (_environment.IsDevelopment())
            {
                response = ApiResponse<object>.ErrorResponse(
    "An exception occurred",
    new List<string>
    {
        errorMessage,
        $"Exception Type: {exception.GetType().Name}",
        $"Stack Trace: {exception.StackTrace}"
    });

            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Maps exception types to appropriate HTTP status codes and user-friendly messages
        /// </summary>
        private (HttpStatusCode statusCode, string message) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                // Domain/Business exceptions → 400 Bad Request
                DomainException domainEx => (
                    HttpStatusCode.BadRequest,
                    domainEx.Message
                ),

                // Authentication failures → 401 Unauthorized
                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "You are not authorized to perform this action."
                ),

                // Not found exceptions → 404 Not Found
                KeyNotFoundException => (
                    HttpStatusCode.NotFound,
                    "The requested resource was not found."
                ),

                ArgumentNullException argNullEx => (
                    HttpStatusCode.NotFound,
                    $"Required resource not found: {argNullEx.ParamName}"
                ),

                // Validation exceptions → 400 Bad Request
                ArgumentException argEx => (
                    HttpStatusCode.BadRequest,
                    argEx.Message
                ),

                InvalidOperationException invalidOpEx => (
                    HttpStatusCode.BadRequest,
                    invalidOpEx.Message
                ),

                // Default: Internal Server Error
                _ => (
                    HttpStatusCode.InternalServerError,
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later."
                )
            };
        }
    }
}