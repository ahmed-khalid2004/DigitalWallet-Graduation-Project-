namespace DigitalWallet.Application.DTOs.Transfer
{
    public class TransferResponseDto
    {
        public Guid TransferId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TransferredAt { get; set; }
    }
}