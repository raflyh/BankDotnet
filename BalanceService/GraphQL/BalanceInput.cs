namespace BalanceService.GraphQL
{
    public record BalanceInput
    (
         int? Id,
         int? UserId,
         string AccountNumber,
         double TotalBalance,
         DateTime? CreatedDate
     );
}
