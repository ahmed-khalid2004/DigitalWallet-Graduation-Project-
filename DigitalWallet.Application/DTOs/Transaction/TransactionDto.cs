using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.Transaction
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}