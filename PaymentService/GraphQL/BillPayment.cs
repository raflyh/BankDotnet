namespace PaymentService.GraphQL
{
    public record BillPayment
    (
        int? Id,
        string? VirtualAccount,
        double? TotalBill,
        string? PaymentStatus,
        string? Type
    );
}
