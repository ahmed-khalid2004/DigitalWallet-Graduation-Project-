namespace DigitalWallet.Application.DTOs.Wallet
{
    public class CreateWalletRequestDto
    {
        public Guid UserId { get; set; }
        public string CurrencyCode { get; set; } = "EGP";
    }
}