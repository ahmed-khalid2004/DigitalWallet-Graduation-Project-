namespace DigitalWallet.Application.Common
{
    /// <summary>
    /// Generic service result wrapper for service layer operations.
    /// </summary>
    public class ServiceResult<T>
    {
        /// <summary>
        /// Indicates whether the service operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The data returned by the service (null if operation failed).
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// The main error message (populated only if IsSuccess is false).
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// List of all error messages.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Optional message for additional context.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Creates a successful service result with data.
        /// </summary>
        public static ServiceResult<T> Success(T data, string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                ErrorMessage = null,
                Errors = new List<string>()
            };
        }

        /// <summary>
        /// Creates a failed service result with a single error message.
        /// </summary>
        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = errorMessage,
                Message = null,
                Errors = new List<string> { errorMessage }
            };
        }

        /// <summary>
        /// Creates a failed service result with multiple error messages.
        /// </summary>
        public static ServiceResult<T> Failure(List<string> errors)
        {
            var errorMessage = errors != null && errors.Any()
                ? string.Join(", ", errors)
                : "Operation failed";

            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessage = errorMessage,
                Message = null,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Non-generic service result for operations that don't return data.
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// Indicates whether the service operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The main error message (populated only if IsSuccess is false).
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// List of all error messages.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Optional message for additional context.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Creates a successful service result.
        /// </summary>
        public static ServiceResult Success(string? message = null)
        {
            return new ServiceResult
            {
                IsSuccess = true,
                Message = message,
                ErrorMessage = null,
                Errors = new List<string>()
            };
        }

        /// <summary>
        /// Creates a failed service result with a single error message.
        /// </summary>
        public static ServiceResult Failure(string errorMessage)
        {
            return new ServiceResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Message = null,
                Errors = new List<string> { errorMessage }
            };
        }

        /// <summary>
        /// Creates a failed service result with multiple error messages.
        /// </summary>
        public static ServiceResult Failure(List<string> errors)
        {
            var errorMessage = errors != null && errors.Any()
                ? string.Join(", ", errors)
                : "Operation failed";

            return new ServiceResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Message = null,
                Errors = errors ?? new List<string>()
            };
        }
    }
}