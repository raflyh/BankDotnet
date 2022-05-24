using Database.Models;
using HotChocolate.AspNetCore.Authorization;

namespace BalanceService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public IQueryable<Balance> GetAllProduct([Service] BankDotnetDbContext context) =>
              context.Balances;
    }
}
