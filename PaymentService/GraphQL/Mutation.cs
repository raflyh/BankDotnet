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

            var newTransaction = new Transaction
            {
                CreditId = customerCredit.Id,
                BalanceId = customerBalance.Id,
                Total = 0,
                TransactionDate = DateTime.Now,
                Description = "Payment for Travika",
                CreatedDate = DateTime.Now
            };
            var bill = context.Bills.Where(o => o.PaymentStatus == "Unpaid").FirstOrDefault();

            var newBill = new Bill
            {
                TransactionId = newTransaction.Id,
                VirtualAccount = input.VirtualAccount,
                TotalBill = (double)input.TotalBill,
                PaymentStatus = "Paid",
                Type = input.Type
            };
        }
    }
}
