namespace DigitalWallet.Application.DTOs.Transaction
{
    public class TransactionHistoryDto
    {
        public List<TransactionDto> Transactions { get; set; } = new();
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public int TotalCount { get; set; }
    }
}