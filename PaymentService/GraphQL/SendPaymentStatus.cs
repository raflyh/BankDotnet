namespace PaymentService.GraphQL
{
    public class SendPaymentStatus
    {
        public int? TransactionId { get; set; }
        public string? VirtualAccount { get; set; }
        public string? Bills { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
