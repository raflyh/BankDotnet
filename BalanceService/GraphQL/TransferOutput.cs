namespace BalanceService.GraphQL
{
    public class TransferOutput
    {
        public string TransactionDate { set; get; }
        public string Message { set; get; }
        public string? SenderAccount { set; get; }
        public string? RecipientAccount { set; get; }
        public string? Description { set; get; }
    }
}
