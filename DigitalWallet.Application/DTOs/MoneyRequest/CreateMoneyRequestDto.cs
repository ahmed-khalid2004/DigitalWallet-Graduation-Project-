namespace DigitalWallet.Application.DTOs.MoneyRequest
{
    public class CreateMoneyRequestDto
    {
        public string ToUserPhoneOrEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "EGP";
    }
}