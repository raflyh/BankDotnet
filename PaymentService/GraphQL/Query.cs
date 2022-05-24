using Database.Models;
using HotChocolate.AspNetCore.Authorization;
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
            if (user != null)
            {
                if (userRole != null)
                {
                    if (userRole.Value == "MANAGER")
                    {
                        return context.Bills.AsQueryable();
                    }
                }
                var orders = context.Bills.Where(a => a.TransactionId == user.Id);
                return orders.AsQueryable();
            }
            return new List<Bill>().AsQueryable();
        }
    }
}
