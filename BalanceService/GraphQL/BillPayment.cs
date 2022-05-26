namespace BalanceService.GraphQL
{
    public record BillPayment
    (
        int? Id,
        int? TransactionId,
        string? VirtualAccount,
        double? TotalBill,
        string? PaymentStatus,
        string? Type
    );
}
