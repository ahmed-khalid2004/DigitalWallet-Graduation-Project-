using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.Admin
{
    public class FraudLogDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public FraudType Type { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}