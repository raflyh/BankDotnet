using Database.Models;

namespace BalanceService.GraphQL
{
    public class Query
    {
        //[Authorize(Roles = new[] { "CUSTOMER" })]
        public IQueryable<Balance> GetAllProduct([Service] BankDotnetDbContext context) =>
              context.Balances;
    }
}
