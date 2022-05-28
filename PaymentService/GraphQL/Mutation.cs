using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentService.Helpers;
using PaymentService.Settings;
using System.Security.Claims;

namespace PaymentService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TransactionStatus> PayTravikaAsync(
            BillPayment input,
            ClaimsPrincipal claimsPrincipal,
            [Service] IOptions<KafkaSettings> settings,
            [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var customer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customerBalance = context.Balances.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var customerCredit = context.Credits.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var travika = context.Users.Where(o => o.Username.Contains("Travika")).FirstOrDefault();
            var travikaBalance = context.Balances.Where(o => o.UserId == travika.Id).OrderBy(o => o.Id).LastOrDefault();

            var bill = context.Bills.Where(o => o.PaymentStatus == "Accepted" && o.VirtualAccount == input.VirtualAccount).FirstOrDefault();//Sample
            if (bill != null)
            {
                if (input.Type == "Balance")
                {
                    if (customerBalance.TotalBalance >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = travikaBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Travika Bill",
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
                        bill.BalanceId = customerBalance.Id;
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();
                        //send kafka
                        var sendPaymentStatus = new SendPaymentStatus
                        {
                            VirtualAccount = bill.VirtualAccount,
                            Bills = Convert.ToString(bill.TotalBill),
                            PaymentStatus = bill.PaymentStatus,
                            TransactionId = bill.BillTransactionId
                        };
                        var key = "Payment-Status-" + DateTime.Now.ToString();
                        var val = JsonConvert.SerializeObject(sendPaymentStatus);
                        Console.WriteLine("====Sending Payment Status====");
                        await KafkaHelper.SendPaymentStatus(settings.Value, "TRAVIKA", key, val);
                        Console.WriteLine("====Payment Status Sent====");
                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                        (
                            false, "Balance Insufficient!"
                        ));
                }
                if (input.Type == "Credit")
                {
                    double custLimit = customerCredit.Limit - customerCredit.TotalCredit;//check current limit
                    if (custLimit >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            CreditId = customerCredit.Id,
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = travikaBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Travika Bill",
                        };
                        context.Transactions.Add(newTransaction);

                        customerCredit.TotalCredit = customerCredit.TotalCredit + bill.TotalBill;
                        context.Credits.Update(customerCredit);

                        var newTravikaBalance = new Balance
                        {
                            UserId = travikaBalance.UserId,
                            AccountNumber = travikaBalance.AccountNumber,
                            TotalBalance = travikaBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newTravikaBalance);

                        //Update bill SQL
                        bill.CreditId = customerCredit.Id;
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();
                        //send kafka
                        var sendPaymentStatus = new SendPaymentStatus
                        {
                            VirtualAccount = bill.VirtualAccount,
                            Bills = Convert.ToString(bill.TotalBill),
                            PaymentStatus = bill.PaymentStatus,
                            TransactionId = bill.BillTransactionId
                        };
                        var key = "Payment-Status-" + DateTime.Now.ToString();
                        var val = JsonConvert.SerializeObject(sendPaymentStatus);
                        Console.WriteLine("====Sending Payment Status====");
                        await KafkaHelper.SendPaymentStatus(settings.Value, "TRAVIKA", key, val);
                        Console.WriteLine("====Payment Status Sent====");

                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                        (
                            false, "Credit Denied!"
                        ));
                }
            }
            
            return await Task.FromResult(new TransactionStatus
                (
                    false, "Travika Bill not Found!"
                ));
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TransactionStatus> PaySolakaAsync(
            BillPayment input,
            ClaimsPrincipal claimsPrincipal,
            [Service] IOptions<KafkaSettings> settings,
            [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var customer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customerBalance = context.Balances.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var customerCredit = context.Credits.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var solaka = context.Users.Where(o => o.Username.Contains("Solaka")).FirstOrDefault();
            var solakaBalance = context.Balances.Where(o => o.UserId == solaka.Id).OrderBy(o => o.Id).LastOrDefault();


            var bill = context.Bills.Where(o => o.PaymentStatus == "Accepted" && o.VirtualAccount == input.VirtualAccount).FirstOrDefault();//Sample
            if (bill != null)
            {
                if (input.Type == "Balance")
                {
                    if (customerBalance.TotalBalance >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = solakaBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Solaka Bill",
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
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();
                        //send kafka
                        var sendPaymentStatus = new SendPaymentStatus
                        {
                            VirtualAccount = bill.VirtualAccount,
                            Bills = Convert.ToString(bill.TotalBill),
                            PaymentStatus = bill.PaymentStatus,
                            TransactionId = bill.BillTransactionId
                        };
                        var key = "Payment-Status-" + DateTime.Now.ToString();
                        var val = JsonConvert.SerializeObject(sendPaymentStatus);
                        Console.WriteLine("====Sending Payment Status====");
                        await KafkaHelper.SendPaymentStatus(settings.Value, "SOLAKA", key, val);
                        Console.WriteLine("====Payment Status Sent====");
                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                            (
                                false, "Balance Insufficient!"
                            ));
                }
                if (input.Type == "Credit")
                {
                    double custLimit = customerCredit.Limit - customerCredit.TotalCredit;//check current limit
                    if (custLimit >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            CreditId = customerCredit.Id,
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = solakaBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Solaka Bill",
                        };
                        context.Transactions.Add(newTransaction);

                        customerCredit.TotalCredit = customerCredit.TotalCredit + bill.TotalBill;
                        context.Credits.Update(customerCredit);

                        var newSolakaBalance = new Balance
                        {
                            UserId = solakaBalance.UserId,
                            AccountNumber = solakaBalance.AccountNumber,
                            TotalBalance = solakaBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newSolakaBalance);

                        //Update bill SQL
                        bill.CreditId = customerCredit.Id;
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();
                        //send kafka
                        var sendPaymentStatus = new SendPaymentStatus
                        {
                            VirtualAccount = bill.VirtualAccount,
                            Bills = Convert.ToString(bill.TotalBill),
                            PaymentStatus = bill.PaymentStatus,
                            TransactionId = bill.BillTransactionId
                        };
                        var key = "Payment-Status-" + DateTime.Now.ToString();
                        var val = JsonConvert.SerializeObject(sendPaymentStatus);
                        Console.WriteLine("====Sending Payment Status====");
                        await KafkaHelper.SendPaymentStatus(settings.Value, "SOLAKA", key, val);
                        Console.WriteLine("====Payment Status Sent====");

                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                        (
                            false, "Credit Denied!"
                        ));
                }
            }
            
            return await Task.FromResult(new TransactionStatus
                (
                    false, "Solaka Bill not Found!"
                ));
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TransactionStatus> PayElectricAsync(
            BillPayment input,
            ClaimsPrincipal claimsPrincipal,
            [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var customer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customerBalance = context.Balances.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var customerCredit = context.Credits.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var pln = context.Users.Where(o => o.Username.Contains("PLN")).FirstOrDefault();
            var plnBalance = context.Balances.Where(o => o.UserId == pln.Id).OrderBy(o => o.Id).LastOrDefault();

            var bill = context.Bills.Where(o => o.PaymentStatus == "Unpaid" && o.VirtualAccount == input.VirtualAccount).FirstOrDefault();//Sample
            if (bill != null)
            {
                if (input.Type == "Balance")
                {
                    if (customerBalance.TotalBalance >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = plnBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Electric Bill",
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
                            UserId = plnBalance.UserId,
                            AccountNumber = plnBalance.AccountNumber,
                            TotalBalance = plnBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newSolaBalance);
                        //Update bill SQL
                        bill.BalanceId = customerBalance.Id;
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();

                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                    (
                       false, "Balance Insufficient!"
                    ));
                }
                if (input.Type == "Credit")
                {
                    double custLimit = customerCredit.Limit - customerCredit.TotalCredit;//check current limit
                    if (custLimit >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            CreditId = customerCredit.Id,
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = plnBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Electric Bill",
                        };
                        context.Transactions.Add(newTransaction);

                        customerCredit.TotalCredit = customerCredit.TotalCredit + bill.TotalBill;
                        context.Credits.Update(customerCredit);

                        var newPlnBalance = new Balance
                        {
                            UserId = plnBalance.UserId,
                            AccountNumber = plnBalance.AccountNumber,
                            TotalBalance = plnBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newPlnBalance);

                        //Update bill SQL
                        bill.CreditId = customerCredit.Id;
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();

                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                        (
                            false, "Credit Denied!"
                        ));
                }
            }
            return await Task.FromResult(new TransactionStatus
                (
                    false, "Electric Bill not Found!"
                ));
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TransactionStatus> PayWaterAsync(
            BillPayment input,
            ClaimsPrincipal claimsPrincipal,
            [Service] BankDotnetDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var customer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customerBalance = context.Balances.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var customerCredit = context.Credits.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var pdam = context.Users.Where(o => o.Username.Contains("PDAM")).FirstOrDefault();
            var pdamBalance = context.Balances.Where(o => o.UserId == pdam.Id).OrderBy(o => o.Id).LastOrDefault();

            var bill = context.Bills.Where(o => o.PaymentStatus == "Unpaid" && o.VirtualAccount == input.VirtualAccount).FirstOrDefault();//Sample
            if (bill != null)
            {
                if (input.Type == "Balance")
                {
                    if (customerBalance.TotalBalance >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = pdamBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Water Bill",
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

                        var newPdamBalance = new Balance
                        {
                            UserId = pdamBalance.UserId,
                            AccountNumber = pdamBalance.AccountNumber,
                            TotalBalance = pdamBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newPdamBalance);
                        //Update bill SQL
                        bill.BalanceId = customerBalance.Id;
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();

                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                       (
                          false, "Balance Insufficient!"
                       ));
                }
                if (input.Type == "Credit")
                {
                    double custLimit = customerCredit.Limit - customerCredit.TotalCredit;//check current limit
                    if (custLimit >= bill.TotalBill)
                    {
                        var newTransaction = new Transaction
                        {
                            CreditId = customerCredit.Id,
                            SenderBalanceId = customerBalance.Id,
                            RecipientBalanceId = pdamBalance.Id,
                            BillId = bill.Id,
                            Total = bill.TotalBill,
                            TransactionDate = DateTime.Now,
                            Description = "Payment for Water Bill",
                        };
                        context.Transactions.Add(newTransaction);

                        customerCredit.TotalCredit = customerCredit.TotalCredit + bill.TotalBill;
                        context.Credits.Update(customerCredit);

                        var newPdamBalance = new Balance
                        {
                            UserId = pdamBalance.UserId,
                            AccountNumber = pdamBalance.AccountNumber,
                            TotalBalance = pdamBalance.TotalBalance + bill.TotalBill,
                            CreatedDate = DateTime.Now
                        };
                        context.Balances.Add(newPdamBalance);

                        //Update bill SQL
                        bill.CreditId = customerCredit.Id;
                        bill.Type = input.Type;
                        bill.PaymentStatus = "Paid";
                        context.Bills.Update(bill);
                        await context.SaveChangesAsync();

                        return await Task.FromResult(new TransactionStatus
                        (
                            true, "Bill Succesfully Paid!"
                        ));
                    }
                    return await Task.FromResult(new TransactionStatus
                        (
                            false, "Credit Denied!"
                        ));
                }
            }
            return await Task.FromResult(new TransactionStatus
                (
                    false, "Water Bill not Found!"
                ));
        }
    }
}
