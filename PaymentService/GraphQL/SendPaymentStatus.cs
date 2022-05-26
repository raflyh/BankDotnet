namespace PaymentService.GraphQL
{
    public class SendPaymentStatus
    {
        public string? VirtualAccount { get; set; }
        public double? Bills { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
