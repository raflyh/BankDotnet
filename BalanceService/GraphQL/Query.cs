using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BalanceService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public IQueryable<Balance> GetAllProduct([Service] BankDotnetDbContext context) =>
              context.Balances;

        [Authorize]
        public IQueryable<User> GetBalanceWithCondition(ClaimsPrincipal claimsPrincipal, [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;

            var userRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var userBalance = context.Balances.Where(o => o.UserId == user.Id).FirstOrDefault();
            if (user != null)
            {
                if (userRole != null)
                {
                    if (userRole.Value == "MANAGER" || userRole.Value == "CUSTOMER SERVICE")
                    {
                        return context.Users.Include(o => o.Balances).AsQueryable();
                    }
                }
                var userWithBalance = context.Users.Include(o => o.Balances).Where(o => o.Id == userBalance.UserId);
                return userWithBalance.AsQueryable();
            }
            return new List<User>().AsQueryable();
        }
    }
    
}
