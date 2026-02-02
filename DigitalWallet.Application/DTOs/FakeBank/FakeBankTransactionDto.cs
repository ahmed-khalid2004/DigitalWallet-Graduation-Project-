using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.FakeBank
{
    public class FakeBankTransactionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public int DelaySeconds { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}