using Database.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

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

                //DateTime dueTime = new DateTime(0, 0, 30, 0, 0, 0);
                //TimeSpan result = input.DueDate.AddMonths(1); 

                var newCredit = new Credit
                {
                    // EF
                    UserId = input.UserId,
                    Limit = input.Limit,
                    CreatedDate = DateTime.Now,
                    DueDate = DateTime.Now.AddMonths(1),
                    TotalCredit = 0,
                    /*TotalBalance = saldo.TotalBalance,*/ //pemanggilan totalBalance
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
                credit.DueDate = DateTime.Now.AddMonths(1);

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
        
        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<CreditOutput> PaydebtCreditAsync(
            PaymentWithCredit input,
            [Service] BankDotnetDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == username).FirstOrDefault();
            var balance = context.Balances.Where(a => a.UserId == user.Id).FirstOrDefault();
            var credit = context.Credits.Where(a => a.UserId == user.Id).FirstOrDefault();

            TimeSpan result = DateTime.Now.Subtract(credit.DueDate);


            if (credit != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    if (balance.TotalBalance > input.amountCredit)
                    {
                        if (credit.DueDate < DateTime.Now)
                        {
                            credit.TotalCredit = credit.TotalCredit + (result.TotalDays * 0.005 * credit.TotalCredit); //bunga per hari
                        }

                        credit.TotalCredit = credit.TotalCredit - input.amountCredit;
                        //credit.TotalBalance = credit.TotalBalance - input.amountCredit;
                        context.Credits.Update(credit);
                        context.SaveChanges();

                        balance.TotalBalance = balance.TotalBalance - input.amountCredit;
                        context.Balances.Update(balance);
                        context.SaveChanges();

                        var transac = new Transaction
                        {
                            CreditId = credit.Id,
                            Total = input.amountCredit,
                            TransactionDate = DateTime.Now
                        };
                        context.Transactions.Add(transac);
                        context.SaveChanges();

                        transaction.Commit();

                        return new CreditOutput
                        {
                            TransactionDate = DateTime.Now.ToString(),
                            Message = "Payment with Credit already succeed.",
                            CreditNumber = credit.CreditNumber
                        };
                    }
                }
                catch (Exception err)
                {
                    transaction.Rollback();
                }
            }
            return new CreditOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Payment with Credit failed"
            };

        }
        //public async Task<CreditOutput> AddPaymentWithCreditAsync(
        //    PaymentWithCredit input,
        //    [Service] BankDotnetDbContext context,
        //    ClaimsPrincipal claimsPrincipal)
        //{
        //    var username = claimsPrincipal.Identity.Name;
        //    var user = context.Users.Where(o => o.Username == username).FirstOrDefault();
        //    var balance = context.Balances.Where(a => a.UserId == user.Id).FirstOrDefault();
        //    var credit = context.Credits.Where(a => a.UserId == user.Id).FirstOrDefault();

        //    if (credit != null)
        //    {
        //        if(credit.Limit > (credit.TotalCredit + input.amountCredit))
        //        {
        //            credit.TotalCredit = credit.TotalCredit + input.amountCredit;

        //            context.Credits.Update(credit);

        //            await context.SaveChangesAsync();
        //            return new CreditOutput
        //            {
        //                TransactionDate = DateTime.Now.ToString(),
        //                Message = "Payment with Credit already succeed.",
        //                CreditNumber = credit.CreditNumber
        //            };
        //        }
        //        return new CreditOutput 
        //        { 
        //            TransactionDate = DateTime.Now.ToString(),
        //            Message = "Payment with Credit failed"
        //        };
        //    }
        //    return new CreditOutput
        //    {
        //        TransactionDate = DateTime.Now.ToString(),
        //        Message = "Payment with Credit failed"
        //    };

        //}

    }
}
