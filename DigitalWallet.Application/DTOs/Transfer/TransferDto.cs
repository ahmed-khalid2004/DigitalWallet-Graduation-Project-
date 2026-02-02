using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.Transfer
{
    public class TransferDto
    {
        public Guid Id { get; set; }
        public Guid SenderWalletId { get; set; }
        public Guid ReceiverWalletId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}