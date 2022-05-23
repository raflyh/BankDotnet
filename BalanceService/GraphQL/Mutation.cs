using Database.Models;
using HotChocolate.AspNetCore.Authorization;

namespace BalanceService.GraphQL
{
    public class Mutation
    {
        //[Authorize(Roles = new[] { "CUSTOMER" })]
        public async Task<Balance> AddBalanceAsync(
            BalanceInput input,
            [Service] BankDotnetDbContext context)
        {

            // EF
            var balance = new Balance
            {
                UserId = input.UserId,
                AccountNumber = Guid.NewGuid().ToString() + input.UserId.ToString(),
                TotalBalance = input.TotalBalance,
                CreatedDate = DateTime.Now,
            };

            var ret = context.Balances.Add(balance);
            await context.SaveChangesAsync();

            return ret.Entity;
        }
    }
}
