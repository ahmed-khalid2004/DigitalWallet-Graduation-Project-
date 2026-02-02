namespace DigitalWallet.Application.DTOs.FakeBank
{
    public class WithdrawRequestDto
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}