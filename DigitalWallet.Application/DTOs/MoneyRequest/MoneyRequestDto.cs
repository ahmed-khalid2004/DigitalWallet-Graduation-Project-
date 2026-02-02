using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.MoneyRequest
{
    public class MoneyRequestDto
    {
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public string FromUserName { get; set; } = string.Empty;
        public Guid ToUserId { get; set; }
        public string ToUserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public MoneyRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}