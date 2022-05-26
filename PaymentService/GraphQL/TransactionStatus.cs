namespace PaymentService.GraphQL
{
    public record TransactionStatus
    (
        bool? success,
        string? message
    );
}
