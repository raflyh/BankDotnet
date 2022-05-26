namespace BalanceService.GraphQL
{
    public record TransferBalance
    (
        string RecipientAccountNumber,
        string? SenderAccountNumber,
        double Total,
        string? Description
    );
}
