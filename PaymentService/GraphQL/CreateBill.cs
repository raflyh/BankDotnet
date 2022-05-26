namespace PaymentService.GraphQL
{
    public record CreateBill
    (
        int? Id,
        int? CreditId,
        int? BalanceId,
        string VirtualAccount,
        double TotalBill,
        string? PaymentStatus,
        string Type
    );
}
