namespace DigitalWallet.Application.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public Guid UserId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}