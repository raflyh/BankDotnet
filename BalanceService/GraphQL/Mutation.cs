using BalanceService.Setting;
using Database.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;

namespace BalanceService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public async Task<ResponseOutput> AddBalanceAsync(
            BalanceInput input,
            [Service] BankDotnetDbContext context)
        {
            var nasabah = context.Balances.Where(x => x.AccountNumber == input.AccountNumber).FirstOrDefault();
            var user = context.Users.Where(o=>o.Id == nasabah.UserId).FirstOrDefault();
            if (nasabah != null)
            {
                nasabah.TotalBalance = (nasabah.TotalBalance + input.TotalBalance);
                nasabah.CreatedDate = DateTime.Now;

                var ret = context.Balances.Update(nasabah);
                await context.SaveChangesAsync();
                
                return new ResponseOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = $"Berhasil Menambah Saldo ke No Rekening: {nasabah.AccountNumber}, Nama: {user.FullName}",
                };
            }
            return new ResponseOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = $"Data Nasabah Dengan No Rekening : {nasabah.AccountNumber} Tidak Ditemukan",
            };
        }

        [Authorize(Roles = new[] { "CUSTOMER SERVICE" })]
        public async Task<TransferOutput> AddTransferWithCSAsync(
            TransferBalance input,
            [Service] BankDotnetDbContext context)
        {
            var recipient = context.Balances.Where(x => x.AccountNumber == input.RecipientAccountNumber).FirstOrDefault();
            var userRecipient = context.Users.Where(o => o.Id == recipient.UserId).FirstOrDefault();
            var sender = context.Balances.Where(s => s.AccountNumber == input.SenderAccountNumber).FirstOrDefault();
            var userSender = context.Users.Where(o => o.Id == sender.UserId).FirstOrDefault();

            if (sender == null)
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Penerima Tidak Ditemukan"
                };
            if (recipient == null)
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message ="Pengirim Tidak Ditemukan"
                };
            if (recipient != null && sender != null)
            {
                if (sender.TotalBalance > input.Total)
                {
                    var transaksi = new Transaction
                    {
                        RecipientBalanceId = recipient.Id,
                        SenderBalanceId= sender.Id,
                        Total= input.Total,
                        TransactionDate = DateTime.Now,
                        Description = input.Description
                    };
                    context.Transactions.Add(transaksi);

                    sender.TotalBalance = (sender.TotalBalance - input.Total);
                    context.Balances.Update(sender);

                    recipient.TotalBalance = (recipient.TotalBalance + input.Total);
                    context.Balances.Update(recipient);
                    await context.SaveChangesAsync();

                    return new TransferOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Transfer Berhasil",
                        SenderAccount = $"{userSender.FullName}-{sender.AccountNumber}",
                        RecipientAccount = $"{userRecipient.FullName}-{recipient.AccountNumber}",
                        Description = input.Description,
                    };

                }
                else
                {
                    return new TransferOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Saldo Tidak Mencukupi"
                    };
                }
            }
            else
            {
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Error"
                };
            }
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TransferOutput> AddTransferAsync(
            TransferBalance input,
            [Service] BankDotnetDbContext context,ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var recipient = context.Balances.Where(x => x.AccountNumber == input.RecipientAccountNumber).FirstOrDefault();
            var userRecipient = context.Users.Where(o => o.Id == recipient.UserId).FirstOrDefault();
            var userSender = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var sender = context.Balances.Where(s => s.UserId == userSender.Id).FirstOrDefault();

            if (sender == null)
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Penerima Tidak Ditemukan"
                };
            if (recipient == null)
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Pengirim Tidak Ditemukan"
                };
            if (recipient != null && sender != null)
            {
                if (sender.TotalBalance > input.Total)
                {
                    var transaksi = new Transaction
                    {
                        RecipientBalanceId = recipient.Id,
                        SenderBalanceId = sender.Id,
                        Total = input.Total,
                        TransactionDate = DateTime.Now,
                        Description = input.Description
                    };
                    context.Transactions.Add(transaksi);

                    sender.TotalBalance = (sender.TotalBalance - input.Total);
                    context.Balances.Update(sender);

                    recipient.TotalBalance = (recipient.TotalBalance + input.Total);
                    context.Balances.Update(recipient);
                    await context.SaveChangesAsync();

                    return new TransferOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Transfer Berhasil",
                        SenderAccount = $"{userSender.FullName}-{sender.AccountNumber}",
                        RecipientAccount = $"{userRecipient.FullName}-{recipient.AccountNumber}",
                        Description = input.Description,
                    };
                }
                else
                {
                    return new TransferOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Saldo Tidak Mencukupi"
                    };
                }
            }
            else
            {
                return new TransferOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Error"
                };
            }
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TopupOutput> AddRedeemCodeAsync(
            TopupOpo input,
            ClaimsPrincipal claimsPrincipal,
            [Service] BankDotnetDbContext context, [Service] IOptions<KafkaSettings> settings)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(s=>s.Username == userName).FirstOrDefault();
            var balance = context.Balances.Where(s=>s.UserId == user.Id).FirstOrDefault();
            //create code generator
            const string src = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 16;
            var sb = new StringBuilder();
            Random rdm = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[rdm.Next(0, src.Length)];
                sb.Append(c);
            }
            //input
            input.Code = sb.ToString();
            input.Amount = input.Amount;
            //send kafka
            var dts = DateTime.Now.ToString();
            var key = "RedeemCode-" + dts;
            var val = JObject.FromObject(input).ToString(Formatting.None);/*JsonConvert.SerializeObject(input);*/
            var result = await KafkaHelper.SendMessage(settings.Value, "simpleOrder", key, val);
            
            TopupOutput resp = new TopupOutput
            {
                TransactionDate = dts,
                Message = "Create redeem code successful"
            };
            if (!result)
                resp.Message = "Failed to submit data";

            //update balance
            balance.TotalBalance = balance.TotalBalance - input.Amount;
            context.Balances.Update(balance);
            context.SaveChanges();
            //add transaksi
            var transaksi = new Transaction
            {
                SenderBalanceId = balance.Id,
                Total = input.Amount,
                Description = $"Create Redeem Code - {user.FullName}",
                TransactionDate = DateTime.Now,
            };
            context.Transactions.Add(transaksi); 
            context.SaveChanges();

            return await Task.FromResult(resp);
        }

        [Authorize(Roles = new[] { "NASABAH" })]
        public async Task<TransactionOutput> PaymentOpoAsync(
            BillPayment input,
            [Service] BankDotnetDbContext context, ClaimsPrincipal claimsPrincipal, [Service] IOptions<KafkaSettings> settings)
        {
            var userName = claimsPrincipal.Identity.Name;
            var opo = context.Users.Where(o => o.Username.Contains("OPO")).FirstOrDefault();
            var customer = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var customerBalance = context.Balances.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var customerCredit = context.Credits.Where(o => o.UserId == customer.Id).OrderBy(o => o.Id).LastOrDefault();
            var opoBalance = context.Balances.Where(o => o.UserId == opo.Id).FirstOrDefault();
            
            var bill = context.Bills.Where(o => o.PaymentStatus != "Paid" && o.Type == "Pembayaran OPO").FirstOrDefault();//Sample
            if (bill == null)
            {
                return new TransactionOutput
                {
                    Message = "Pembayaran Gagal",
                    Status = false,
                    TransactionDate = DateTime.Now.ToString(),
                };
            }
            if (bill.VirtualAccount == input.VirtualAccount)
            {
                if (customerBalance.TotalBalance >= bill.TotalBill)
                {
                    var newTransaction = new Transaction
                    {
                        SenderBalanceId = customerBalance.Id,
                        RecipientBalanceId = opoBalance.Id,
                        BillId = bill.Id,
                        Total = bill.TotalBill,
                        TransactionDate = DateTime.Now,
                        Description = "OPO Payment for Electric Bill",
                    };
                    context.Transactions.Add(newTransaction);

                    customerBalance.TotalBalance = customerBalance.TotalBalance - bill.TotalBill;
                    context.Balances.Update(customerBalance);

                    opoBalance.TotalBalance = opoBalance.TotalBalance + bill.TotalBill;
                    context.Balances.Update(opoBalance);

                    bill.VirtualAccount = input.VirtualAccount;
                    bill.BalanceId = customerBalance.Id;
                    bill.PaymentStatus = "Paid";
                    context.Bills.Update(bill);
                    await context.SaveChangesAsync();

                    var sendBill = context.Bills.Where(s => s.VirtualAccount == input.VirtualAccount).FirstOrDefault();
                    var recive = new SendKafkaBill
                    {
                        Virtualaccount = input.VirtualAccount,
                        TransactionId = sendBill.BillTransactionId,
                        Bills = sendBill.TotalBill.ToString(),
                        PaymentStatus = sendBill.PaymentStatus
                    };
                    //send kafka
                    var dts = DateTime.Now.ToString();
                    var key = "RedeemCode-" + dts;
                    var val = JObject.FromObject(recive).ToString(Formatting.None);/*JsonConvert.SerializeObject(input);*/
                    var result = await KafkaHelper.SendMessage(settings.Value, "simpleOrder", key, val);

                    return new TransactionOutput
                    {
                        Message = "Pembayaran Berhasil",
                        Status = true,
                        TransactionDate = DateTime.Now.ToString(),
                    };
                }
                return new TransactionOutput
                {
                    Message = "Pembayaran Gagal",
                    Status = false,
                    TransactionDate = DateTime.Now.ToString(),
                };

            }
            return new TransactionOutput
            {
                Message = "Pembayaran Gagal",
                Status = false,
                TransactionDate = DateTime.Now.ToString(),
            };
        }

    }
}
