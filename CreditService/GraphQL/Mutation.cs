using Database.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;

namespace CreditService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public async Task<Credit> AddCreditAsync(
            CreditData input,
            [Service] BankDotnetDbContext context)
        {
            var nasabah = context.Users.Where(x => x.Id == input.UserId).FirstOrDefault();
            var saldo = context.Balances.Where(o => o.UserId == nasabah.Id).FirstOrDefault();

            var credit = context.Credits.Where(o => o.UserId == nasabah.Id).FirstOrDefault(); //berdasarkan userId
            if (credit == null)
            {
                Random rnd = new Random();
                var random = Convert.ToString(rnd.Next(1000000000,2000000000));
                var newCredit = new Credit
                {
                    // EF
                    UserId = input.UserId,
                    Limit = input.Limit,
                    CreatedDate = DateTime.Now,
                    DueDate = input.DueDate,
                    TotalCredit = 0,
                    TotalBalance = saldo.TotalBalance, //pemanggilan totalBalance
                    CreditNumber ="12" + $"{random}"
                };

                var ret = context.Credits.Add(newCredit);
                context.SaveChanges();

                return ret.Entity;
            }
            return new Credit();
        }
        public async Task<Credit> UpdateCreditAsync(
            CreditData input,
            [Service] BankDotnetDbContext context)
        {
            var nasabah = context.Users.Where(x => x.Id == input.UserId).FirstOrDefault();
            var saldo = context.Balances.Where(o => o.UserId == nasabah.Id).FirstOrDefault();

            var credit = context.Credits.Where(o => o.UserId == nasabah.Id).FirstOrDefault();
            if (credit != null)
            {
                credit.UserId = nasabah.Id;
                credit.Limit = input.Limit;
                credit.DueDate = input.DueDate;

                context.Credits.Update(credit);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(credit);
        }
        public async Task<Credit> DeleteCreditByIdAsync(
            int id,
            [Service] BankDotnetDbContext context)
        {
            var credit = context.Credits.Where(o => o.Id == id).FirstOrDefault();
            if (credit != null)
            {
                context.Credits.Remove(credit);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(credit);
        }
    }
}
