using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace PaymentService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "CUSTOMER" })]
        public async Task<TransactionStatus> PayTravikaAsync(
            BillPayment input,
            [Service] ClaimsPrincipal claimsPrincipal,
            [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var customer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customerBalance = context.Balances.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var customerCredit = context.Credits.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var travika = context.Users.Where(o => o.Username.Contains("Travika")).FirstOrDefault();
            var travikaBalance = context.Balances.Where(o => o.UserId == travika.Id).OrderBy(o => o.Id).LastOrDefault();

            //consume from kafka
            //=====================
            //Here
            //=====================

            var bill = context.Bills.Where(o => o.PaymentStatus == "Unpaid" && o.VirtualAccount.Contains("077")).FirstOrDefault();//Sample
            if (bill != null)
            {
                if(bill.VirtualAccount == input.VirtualAccount)
                {
                    if(input.Type == "Balance")
                    {
                        var newTransaction = new Transaction
                        {
                            CreditId = customerCredit.Id,
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = travikaBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Travika",
                        };
                        context.Transactions.Add(newTransaction);

                        var newCustBalance = new Balance
                        {
                            UserId = customerBalance.UserId,
                            AccountNumber = customerBalance.AccountNumber,
                            TotalBalance = customerBalance.TotalBalance - bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newCustBalance);

                        var newTravBalance = new Balance
                        {
                            UserId = travikaBalance.UserId,
                            AccountNumber = travikaBalance.AccountNumber,
                            TotalBalance = travikaBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newTravBalance);
                        //Update bill
                        bill.VirtualAccount = input.VirtualAccount;
                        bill.BalanceId = customerBalance.Id;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();
                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    if(input.Type == "Credit")
                    {
                        //
                    }
                    return await Task.FromResult(new TransactionStatus
                    (
                        false, "Virtual Account Invalid!"
                    ));

                }
            }
            return await Task.FromResult(new TransactionStatus
                (
                    false, "Travika Bill not Found!"
                ));
        }

        [Authorize(Roles = new[] { "CUSTOMER" })]
        public async Task<TransactionStatus> PaySolakaAsync(
            BillPayment input,
            [Service] ClaimsPrincipal claimsPrincipal,
            [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var customer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customerBalance = context.Balances.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var customerCredit = context.Credits.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var solaka = context.Users.Where(o => o.Username.Contains("Solaka")).FirstOrDefault();
            var solakaBalance = context.Balances.Where(o => o.UserId == solaka.Id).OrderBy(o => o.Id).LastOrDefault();

            //consume from kafka
            //=====================
            //Here
            //=====================

            var bill = context.Bills.Where(o => o.PaymentStatus == "Unpaid" && o.VirtualAccount.Contains("078")).FirstOrDefault();//Sample
            if (bill != null)
            {
                if (bill.VirtualAccount == input.VirtualAccount)
                {
                    if (input.Type == "Balance")
                    {
                        var newTransaction = new Transaction
                        {
                            CreditId = customerCredit.Id,
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = solakaBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Solaka",
                        };
                        context.Transactions.Add(newTransaction);

                        var newCustBalance = new Balance
                        {
                            UserId = customerBalance.UserId,
                            AccountNumber = customerBalance.AccountNumber,
                            TotalBalance = customerBalance.TotalBalance - bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newCustBalance);

                        var newSolaBalance = new Balance
                        {
                            UserId = solakaBalance.UserId,
                            AccountNumber = solakaBalance.AccountNumber,
                            TotalBalance = solakaBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newSolaBalance);
                        //Update bill SQL
                        bill.VirtualAccount = input.VirtualAccount;
                        bill.BalanceId = customerBalance.Id;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();
                        //Send back Status to Kafka
                        //====================
                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    if (input.Type == "Credit")
                    {
                        //
                    }
                    return await Task.FromResult(new TransactionStatus
                    (
                        false, "Virtual Account Invalid!"
                    ));

                }
            }
            return await Task.FromResult(new TransactionStatus
                (
                    false, "Travika Bill not Found!"
                ));
        }
    }
}
