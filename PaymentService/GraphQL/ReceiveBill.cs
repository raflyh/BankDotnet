namespace PaymentService.GraphQL
{
    public class ReceiveBill
    {
        public int TransactionId { get; set; }
        public string Virtualaccount { get; set; }
        public string Bills { get; set; }
        public string PaymentStatus { get; set; }
    }
}
