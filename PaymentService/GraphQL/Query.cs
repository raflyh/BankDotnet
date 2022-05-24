using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PaymentService.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<Bill> GetBills(ClaimsPrincipal claimsPrincipal, [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;

            var userRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var userBalance = context.Balances.Where(o => o.UserId == user.Id).FirstOrDefault();
            if (user != null)
            {
                if (userRole != null)
                {
                    if (userRole.Value == "MANAGER" || userRole.Value == "CS")
                    {
                        return context.Bills.Include(o => o.Transactions).AsQueryable();
                    }
                }
                var bills = context.Bills.Include(o => o.Transactions).Where(o => o.BalanceId == userBalance.Id);
                return bills.AsQueryable();
            }
            return new List<Bill>().AsQueryable();
        }
    }
}
