namespace DigitalWallet.Application.DTOs.Admin
{
    public class WalletManagementDto
    {
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal DailyLimit { get; set; }
        public decimal MonthlyLimit { get; set; }
    }
}