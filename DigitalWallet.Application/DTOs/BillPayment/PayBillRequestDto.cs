namespace DigitalWallet.Application.DTOs.BillPayment
{
    public class PayBillRequestDto
    {
        public Guid WalletId { get; set; }
        public Guid BillerId { get; set; }
        public decimal Amount { get; set; }
        public string OtpCode { get; set; } = string.Empty;
    }
}