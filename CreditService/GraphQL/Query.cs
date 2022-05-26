using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CreditService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE", "MANAGER" })]
        public IQueryable<Credit> GetCredits([Service] BankDotnetDbContext context) =>
            context.Credits;

        [Authorize(Roles = new[] { "NASABAH" })]
        public IQueryable<Credit> GetCreditByNasabah([Service] BankDotnetDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == username).FirstOrDefault();
            if (user != null)
            {
                var orders = context.Credits.Where(o => o.UserId == user.Id);
                return orders.AsQueryable();
            }
            return new List<Credit>().AsQueryable();
        }
    }
}
