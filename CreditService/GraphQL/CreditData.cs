namespace CreditService.GraphQL
{
    public record CreditData
    (
        int? Id,
        int UserId,
        float Limit
        );
}
