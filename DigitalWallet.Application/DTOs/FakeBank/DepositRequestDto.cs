namespace DigitalWallet.Application.DTOs.FakeBank
{
    public class DepositRequestDto
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}