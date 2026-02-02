namespace DigitalWallet.Application.DTOs.Transfer
{
    public class SendMoneyRequestDto
    {
        public Guid SenderWalletId { get; set; }
        public string ReceiverPhoneOrEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string OtpCode { get; set; } = string.Empty;
    }
}