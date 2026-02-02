using DigitalWallet.Domain.Enums;

namespace DigitalWallet.Application.DTOs.BillPayment
{
    public class BillerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BillCategory Category { get; set; }
        public bool IsActive { get; set; }
    }
}