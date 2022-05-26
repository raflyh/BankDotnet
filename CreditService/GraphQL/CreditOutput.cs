namespace CreditService.GraphQL
{
    public class CreditOutput
    {
        public string TransactionDate { set; get; }
        public string Message { set; get; }
        public string? CreditNumber { set; get; }
        public string? Description { set; get; }
    }
}
