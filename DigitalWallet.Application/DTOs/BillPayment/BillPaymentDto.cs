using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.BillPayment
{
    public class BillPaymentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public Guid BillerId { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string? ReceiptPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}