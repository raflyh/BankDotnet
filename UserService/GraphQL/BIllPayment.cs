namespace UserService.GraphQL
{
    public record BillPayment
     (
         int? Id,
         double TotalBill,
         string PaymentStatus
     );
}
