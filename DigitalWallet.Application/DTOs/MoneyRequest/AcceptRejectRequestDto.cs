namespace DigitalWallet.Application.DTOs.MoneyRequest
{
    public class AcceptRejectRequestDto
    {
        public Guid RequestId { get; set; }
        public bool Accept { get; set; }
        public string? OtpCode { get; set; }
    }
}