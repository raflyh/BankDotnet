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
                            return await Task.FromResult(new OutputSaving
                            {
                                message = "Succes To Saving ",
                                TotalSaving = savings.TotalSaving,
                                TotalGold = savings.TotalGold,
                                Date = savings.Date
                            });

                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    return await Task.FromResult(new OutputSaving
                    {
                        message = "Saving Unsuccesfully, Balance Insufficient",
                        Date = DateTime.Now
                    });
                }
            }
            return await Task.FromResult(new OutputSaving
            {
                message = "Saving Unsuccesfully",
                Date = DateTime.Now
            });
        }


        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<OutputSaving> AddSavingsGoldAsync(
            InputSaving input,
            [Service] BankDotnetDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {

            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u => u.Username == Username).FirstOrDefault();
            var balance = context.Balances.Where(i => i.UserId == user.Id).FirstOrDefault();

            if (balance != null)
            {
                if (balance.TotalBalance > input.Saving)
                {
                    var savings = context.Savings.Where(b => b.BalanceId == balance.Id).FirstOrDefault();
                    var gold = input.Saving / 988000;
                    var transaction = context.Database.BeginTransaction();
                    try
                    {
                        if (savings.BalanceId != 0)
                        {
                            savings.TotalGold = savings.TotalGold + gold;
                            context.Savings.Update(savings);
                            context.SaveChanges();

                            balance.TotalBalance = balance.TotalBalance - input.Saving;
                            context.Balances.Update(balance);
                            context.SaveChanges();

                            transaction.Commit();
                            return await Task.FromResult(new OutputSaving
                            {
                                message = "Succes To Saving Gold ",
                                TotalSaving = savings.TotalSaving,
                                TotalGold = savings.TotalGold,
                                Date = savings.Date
                            });

                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    return await Task.FromResult(new OutputSaving
                    {
                        message = "Saving Unsuccesfully, Balance Insufficient",
                        Date = DateTime.Now
                    });
                }
            }

            return await Task.FromResult(new OutputSaving
            {
                message = "Saving Unsuccesfully",
                Date = DateTime.Now
            });
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<OutputSaving> MoveSavingToBalanceAsync(
            InputSavingAndGold input,
            [Service] BankDotnetDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {

            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u => u.Username == Username).FirstOrDefault();
            var balance = context.Balances.Where(i => i.UserId == user.Id).FirstOrDefault();

            var savings = context.Savings.Where(b => b.BalanceId == balance.Id).FirstOrDefault();
            if (balance != null)
            {
                if (savings.TotalSaving >= input.Saving && savings.TotalGold >= input.Gold)
                {
                    TimeSpan result = DateTime.Now.Subtract(savings.Date);
                    savings.Date = DateTime.Now;
                    savings.TotalSaving = savings.TotalSaving + (result.TotalMinutes * 0.001 * savings.TotalSaving);
                    var goldPrice = input.Gold * 988000;

                    var transaction = context.Database.BeginTransaction();
                    try
                    {
                        if (savings.BalanceId != 0)
                        {
                            savings.TotalSaving = savings.TotalSaving - input.Saving;
                            savings.TotalGold = savings.TotalGold - input.Gold;
                            context.Savings.Update(savings);
                            context.SaveChanges();

                            balance.TotalBalance = balance.TotalBalance + input.Saving+goldPrice;
                            context.Balances.Update(balance);
                            context.SaveChanges();

                            transaction.Commit();
                            return await Task.FromResult(new OutputSaving
                            {
                                message = "Succes Transfer Saving To Balance",
                                TotalSaving = savings.TotalSaving,
                                TotalGold = savings.TotalGold,
                                Date = savings.Date
                            });

                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
                else
                {
                    return await Task.FromResult(new OutputSaving
                    {
                        message = "Saving Unsuccesfully, Balance Insufficient",
                        TotalSaving = savings.TotalSaving,
                        TotalGold = savings.TotalGold,
                        Date = DateTime.Now
                    });
                }
            }
            return await Task.FromResult(new OutputSaving
            {
                message = "Saving Unsuccesfully",
                Date = DateTime.Now
            });
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
                    return await Task.FromResult(new OutputSaving
                    {
                        message = "Your Total Saving :",
                        TotalSaving = savings.TotalSaving,
                        TotalGold = savings.TotalGold,
                        Date = savings.Date
                    });
                }
            }

            return await Task.FromResult(new OutputSaving
            {
                message = "Process Unsuccesfully",
                Date = DateTime.Now
            });
        }
    }
}
