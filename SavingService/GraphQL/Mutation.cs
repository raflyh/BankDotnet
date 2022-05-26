using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace SavingService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<OutputSaving> AddSavingsAsync(
            InputSaving input,
            [Service] BankDotnetDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {

            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u => u.Username == Username).FirstOrDefault();
            var balance = context.Balances.Where(i => i.UserId == user.Id).FirstOrDefault();

            if (balance != null)
            {
                if(balance.TotalBalance > input.Saving)
                {
                    var savings = context.Savings.Where(b => b.BalanceId == balance.Id).FirstOrDefault();
                    TimeSpan result = DateTime.Now.Subtract(savings.Date);
                    savings.Date = DateTime.Now;

                    var transaction = context.Database.BeginTransaction();
                    try
                    {
                        if (savings.BalanceId != 0)
                        {
                            savings.TotalSaving = savings.TotalSaving + (result.TotalMinutes*0.001*savings.TotalSaving);
                            savings.TotalSaving = savings.TotalSaving + input.Saving;
                            context.Savings.Update(savings);
                            context.SaveChanges();

                            balance.TotalBalance = balance.TotalBalance - input.Saving;
                            context.Balances.Update(balance);
                            context.SaveChanges();

                            transaction.Commit();
                            return await Task.FromResult(new OutputSaving("Saving Succesfully Updated", savings.TotalSaving, savings.Date));
                        
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    return await Task.FromResult(new OutputSaving("Saving Unsuccesfully, Balance Insufficient", input.Saving, DateTime.Now));
                }
            }

            return await Task.FromResult(new OutputSaving("Saving Unsuccesfully", input.Saving, DateTime.Now));
        }


        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<OutputSaving> GetSavingsAsync(
            [Service] BankDotnetDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u => u.Username == Username).FirstOrDefault();
            var balance = context.Balances.Where(i => i.UserId == user.Id).FirstOrDefault();
            if (balance != null)
            {
                var savings = context.Savings.Where(b => b.BalanceId == balance.Id).FirstOrDefault();
                TimeSpan result = DateTime.Now.Subtract(savings.Date);
                savings.Date = DateTime.Now;

                if (savings != null)
                {
                    savings.TotalSaving = savings.TotalSaving + (result.TotalMinutes * 0.001 * savings.TotalSaving);
                    context.Savings.Update(savings);
                    await context.SaveChangesAsync();

                    return await Task.FromResult(new OutputSaving("Your Total Saving is :", savings.TotalSaving, savings.Date));
                }
            }

            return await Task.FromResult(new OutputSaving("Proses Unsuccesfully", 0, DateTime.Now));
        }
    }
}
