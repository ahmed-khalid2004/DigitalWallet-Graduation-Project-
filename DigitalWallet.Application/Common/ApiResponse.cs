namespace DigitalWallet.Application.Common
{
    /// <summary>
    /// Generic API response wrapper for all API endpoints.
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The main message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The actual data payload (null if operation failed).
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// List of error messages (populated only if Success is false).
        /// </summary>
        public List<string>? Errors { get; set; }

        /// <summary>
        /// The timestamp when this response was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Default constructor initializes timestamp.
        /// </summary>
        public ApiResponse()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a successful response with data.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Errors = null,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates an error response with message and optional error list.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors ?? new List<string> { message },
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates an error response with message and array of errors.
        /// Overload to support string[] from ServiceResult.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, string[] errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors?.ToList() ?? new List<string> { message },
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates an error response with a single error message.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = error,
                Data = default,
                Errors = new List<string> { error },
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Non-generic API response for operations that don't return data.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The main message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// List of error messages (populated only if Success is false).
        /// </summary>
        public List<string>? Errors { get; set; }

        /// <summary>
        /// The timestamp when this response was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Default constructor initializes timestamp.
        /// </summary>
        public ApiResponse()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiResponse SuccessResponse(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Errors = null,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates an error response.
        /// </summary>
        public static ApiResponse ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string> { message },
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates an error response with array of errors.
        /// Overload to support string[] from ServiceResult.
        /// </summary>
        public static ApiResponse ErrorResponse(string message, string[] errors)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors?.ToList() ?? new List<string> { message },
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates an error response with a single error.
        /// </summary>
        public static ApiResponse ErrorResponse(string error)
        {
            return new ApiResponse
            {
                Success = false,
                Message = error,
                Errors = new List<string> { error },
                Timestamp = DateTime.UtcNow
            };
        }
    }
}