namespace DigitalWallet.Application.Common
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ServiceResult<T> Success(T data, string? message = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> Failure(string error)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Errors = new List<string> { error }
            };
        }

        public static ServiceResult<T> Failure(List<string> errors)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }
}