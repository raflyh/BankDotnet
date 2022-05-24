using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace SavingService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "ADMIN" })]
        public IQueryable<Saving> GetSavingByAdmin([Service] BankDotnetDbContext context) =>
            context.Savings;


        //[Authorize(Roles = new[] { "NASABAH" })]
        //public IQueryable<Saving?> GetSaving([Service] BankDotnetDbContext context, ClaimsPrincipal claimsPrincipal)
        //{
        //    var Username = claimsPrincipal.Identity.Name;
        //    var user = context.Users.Where(u => u.Username == Username).FirstOrDefault();
        //    var balance = context.Balances.Where(i => i.UserId == user.Id).FirstOrDefault();
        //    if (balance != null)
        //    {
        //        var savings = context.Savings.Where(b => b.BalanceId == balance.Id);
        //        return savings.AsQueryable();
        //    }
        //    return new List<Saving>().AsQueryable();
        //}
    }
}
