namespace DigitalWallet.Application.DTOs.Wallet
{
    public class WalletBalanceDto
    {
        public Guid WalletId { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }
}